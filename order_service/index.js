// TODO: Implement the order service
const amqp = require('amqplib/callback_api');
const db = require("./data/db");
const order = require('./data/schema/order');

const messageBrokerInfo = {
    exchanges: {
        order: 'order_exchange'
    },
    queues: {
        addQueue: 'order_queue'
    },
    routingKeys: {
        createOrder: 'create_order'
    }
}

const createMessageBrokerConnection = () => new Promise((resolve, reject) => {
    amqp.connect('amqp://localhost', (err, conn) => {
        if (err) { reject(err); }
        resolve(conn);
    });
});

const createChannel = connection => new Promise((resolve, reject) => {
    connection.createChannel((err, channel) => {
        if (err) { reject(err); }
        resolve(channel);
    });
});

const configureMessageBroker = channel => {
    const { addQueue } = messageBrokerInfo.queues;
    const inputKey = messageBrokerInfo.routingKeys.createOrder;
    Object.values(messageBrokerInfo.exchanges).forEach(val => {
        channel.assertExchange(val, 'direct', { durable: true });
        channel.assertQueue(addQueue, {durable: true});
        channel.bindQueue(addQueue, val, inputKey)
    });

};

const calcTotal = (orderList) => {
    var total = 0;
    for (var i = 0; i < orderList.length; i++) {
        total += orderList[i].unitPrice * orderList[i].quantity;
    }
    return total;
};

const saveOrder = (dataJson) => {
    const totalPrice = calcTotal(dataJson.items)
    const currentDate = Date.now()
    const newOrder = new db.Order({
        customerEmail: dataJson.email,
        totalPrice: totalPrice,
        orderDate: currentDate
    })
    const orderId = newOrder.id;
    newOrder.save();
    return orderId;
}

const saveOrderItems = async (orderItems, orderId) => {
    for (var i = 0; i < orderItems.length; i++) {
        const orderItem = new db.OrderItem({
            description: orderItems[i].description,
            quantity: orderItems[i].quantity,
            unitPrice: orderItems[i].unitPrice,
            rowPrice: orderItems[i].unitPrice * orderItems[i].quantity,
            orderId: orderId
        });
        orderItem.save();
    }
}

(async () => {
    const messageBrokerConnection = await createMessageBrokerConnection();
    const channel = await createChannel(messageBrokerConnection);

    configureMessageBroker(channel);

    const { addQueue } = messageBrokerInfo.queues;

    // Consume with the orderQue:
    channel.consume(addQueue, data => {
        const dataJson = JSON.parse(data.content.toString())
        // Save the order and return the order ID.
        const orderId = saveOrder(dataJson);
        // Save the orderItems with the orderId.
        saveOrderItems(dataJson.items, orderId);
        // channel.publish(order, createOrder, new Buffer.from(JSON.stringify(dataJson)));
    }, {noAck: true})
})().catch(e => console.error(e));
