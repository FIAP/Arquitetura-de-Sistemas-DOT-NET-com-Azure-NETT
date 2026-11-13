'use strict';
require('dotenv').config();

const PersonRepository = require('../repositories/personRepository');

const redisUrl = process.env.REDIS_URL;

var redis = require('redis');
const client = redis.createClient({
    socket: {
        host: redisUrl,
        port: 6379
    }
});
client.connect().catch(console.error);
exports.get = async (req, res) => {
    try {
        const reply = await client.get('allpersons');

        if (reply) {
            console.log('redis');
            res.send(reply)
        } else {
            console.log('db');

            const person = await PersonRepository.getAll();
            await client.set('allpersons', JSON.stringify(person));
            await client.expire('allpersons', 300);
            res.status(200).send(person);
        }
    } catch (err) {
        res.status(500).send(err);
    }
};

exports.getById = (req, res) => {
    const _id = req.params.id;

    PersonRepository.getById(_id)
        .then((person) => {
            res.status(200).send(person);
        }).catch(err => res.status(500).send(err))
};

exports.post = (req, res) => {
    const vm = req.body;

    PersonRepository.create(vm)
        .then((person) => {
            res.status(200).send(person);
        }).catch(err => res.status(500).send(err))
};

exports.put = (req, res) => {
    const _id = req.params.id;
    const vm = req.body;

    PersonRepository.update(_id, vm)
        .then((person) => {
            res.status(201).send(person);
        }).catch(err => res.status(500).send(err))
};

exports.delete = (req, res) => {
    PersonRepository.delete(req.params.id)
        .then(() => {
            res.status(200).send('registro deletado com sucesso!');
        }).catch(err => console.error.bind(console, `Error ${err}`))
};