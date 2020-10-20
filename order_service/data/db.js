const orderSchema = require('./schema/order');
const orderItemSchema = require('./schema/orderItem');
const mongoose = require('mongoose');

const connection = mongoose.createConnection('mongodb+srv://danield18:Kjani123@cluster0.3oup2.mongodb.net/CactusHeaven?retryWrites=true&w=majority', {
    useNewUrlParser: true,
    useUnifiedTopology: true
});

module.exports = {
    Order: connection.model('Order', orderSchema),
    OrderItem: connection.model('OrderItem', orderItemSchema)
};
