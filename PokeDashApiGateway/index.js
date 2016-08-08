//
// This is the stubbed out Poke Dash API Gateway.  It currently stubs out /poll, /rank, and /tap.
// You can run it on you local machine at http://localhost:3000/
//
var consul = require('consul');
var cors = require('cors');
var request = require('request');
var express = require('express');
var bodyParser = require('body-parser');
var multer = require('multer');
var upload = multer();
var app = express();

var allowCrossDomain = function(req, res, next) {
    console.log('router');

    if ('OPTIONS' == req.method) {
        res.header('Access-Control-Allow-Origin', '*');
        res.header('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE,PATCH,OPTIONS');
        res.header('Access-Control-Allow-Headers', 'Content-Type, Authorization, Content-Length, X-Requested-With');
        res.send(200);
    }
    else {
        next();
    }
};
app.use(allowCrossDomain);
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

// app.all( function (req, res) {

// console.log('hi');
// res.header("Access-Control-Allow-Origin", "*"); // restrict it to the required domain
// res.header("Access-Control-Allow-Methods", "GET,PUT,POST,DELETE,OPTIONS");
// // Set custom headers for CORS
// res.header("Access-Control-Allow-Headers", "Content-type,Accept,X-Custom-Header");

// if (req.method === "OPTIONS") {
// return res.status(200).end();
// }

// next();
// });
// use it before all route definitions
app.use(cors({origin: 'http://c04dghhack5.ds.ad.adp.com'}));


// var corsoptions = {
// origin: '*',
// credentials: true
// };

// cors({credentials: true, origin: true, preflightcontinue: true});
// app.use(cors(corsoptions));


// app.options('/poll', getPoll);


// console.log('hello');





function getPokeDashScan(state) {
    request({
            method: 'GET',
            url: state.pokeDashScanUrl,
            qs : { lat: state.originLat, lng : state.originLng },
            json : true
        },
        function(error, response, body) {
            if (!error && response.statusCode == 200) {
                state.pokemons = body.entities.pokeList;

                state.originResponse.status(200).json(state.pokemons);
                //state.consulClient.catalog.service.nodes('pokedash-uberestimate', lookupPokeDashUberestimate.bind(undefined, state));
            } else {
                state.originResponse.status(503).send(state.pokeDashScanUrl + "' failed");
            }
        });
}




function consulPokeDashScanCallback(state, err, result) {
    if (!err) {
        if (result.length > 0) {
            state.pokeDashScanUrl = 'http://' + result[0].Address + ':' + result[0].ServicePort + '/pokedash-scan';
            console.log("GET /poll: service 'pokedash-scan' available at '" + state.pokeDashScanUrl + "'");
            getPokeDashScan(state);
        } else {
            console.log("GET /poll: service 'pokedash-scan' not available")
            state.originResponse.status(503).send("service 'pokedash-scan' not available");
        }
    } else {
        console.log("GET /poll: consul error");
        state.originResponse.status(503).send('Consul error');
    }
}

function getPoll(req, res) {
    console.log('inside poll');




    var state = {
        originResponse: res,
        originLat: req.query.lat,
        originLng: req.query.lng,
        consulClient: consul()
    };

    console.log('GET /poll: lat=' + state.originLat + ', lng=' + state.originLng);
    state.consulClient.catalog.service.nodes('pokedash-scan', consulPokeDashScanCallback.bind(undefined, state));
}

app.get('/poll', getPoll);


function getPokeDashRank(state) {
    request({
            method: 'GET',
            url: state.pokeDashRankUrl,
            body: state.uberPokemons,
            json: true
        },
        function (error, response, body) {
            if (!error && response.statusCode == 200) {
                state.pokeRanks = body;
                //state.rankedPokemons = pokeRanks.map(function (i) {
                //return uberPokes[i];
                //});
                // state.rankedPokemons = uberPokes;
                state.originResponse.status(200).json(state.pokeRanks);
            } else {
                state.originResponse.status(503).send(state.pokeDashRankUrl + " failed");
            }
        });
}


function lookupPokeDashRank(state) {
    state.consulClient.catalog.service.nodes('pokedash-rank', function (err, result) {
        if (!err) {
            if (result.length > 0) {
                state.pokeDashRankUrl = 'http://' + result[0].Address + ':' + result[0].ServicePort + '/pokedash-rank';
                console.log("GET /poll: service 'pokedash-rank' available at '" + state.pokeDashRankUrl + "'");
                getPokeDashRank(state);
            } else {
                console.log("GET /poll: service 'pokedash-rank' not available")
                state.originResponse.status(503).send("service 'pokedash-rank' not available");

            }
        } else {
            console.log("GET /poll: consul error");
            state.originResponse.status(503).send('consul error');
        }
    });
}




function getPokeDashUberestimate(state) {
    request({
            method: 'GET',
            url: state.pokeDashUberestimateUrl,
            qs: { slat: state.slat, slng: state.slng },
            body: state.uberReqs,
            json: true
        },
        function (error, response, body) {
            if (!error && response.statusCode == 200) {
                state.uberestimate = body;
                state.uberPokemons = state.originPokemon.map(function (obj, i) {
                    obj.uberPrice = state.uberestimate[i].price;
                    obj.uberTime = state.uberestimate[i].time;
                    return obj;
                });
                lookupPokeDashRank(state);
            } else {
                console.log("GET /poll: GET pokedash-uberestimtate failed");
                state.originResponse.status(503).send("'pokedash-uberestimate' failed");
            }
        });
}

function lookupPokeDashUberestimate(state, err, result) {
    state.consulClient.catalog.service.nodes('pokedash-uberestimate', function (err, result) {
        if (!err) {
            if (result.length > 0) {
                state.pokeDashUberestimateUrl = 'http://' + result[0].Address + ':' + result[0].ServicePort + '/pokedash-uberestimate';
                console.log("GET /poll: service 'pokedash-uberestimate' available at '" + state.pokeDashUberestimateUrl +"'");
                getPokeDashUberestimate(state);
            }
            else {
                console.log("GET /poll: Consul: service 'pokedash-uberestimtates' not available");
                state.originResponse.status(404).send("Consul: service 'pokedash-uberestimate' not available");
            }
        } else {
            console.log("GET /poll: service 'pokedash-uberestimtates' not available");
            state.originResponse.status(503).send("service 'pokedash-scan' not available");
        }
    });
}

app.post('/rank', upload.array(), function (req, res) {


    var state = {
        originResponse: res,
        originPokemon: req.body,
        consulClient: consul(),
        slat: req.query.slat,
        slng: req.query.slng
    }

    console.log(state);
    if(state.originPokemon == null)
    {
        state.originResponse.status(503).send("OriginPokemon was null");
    }
    else
    {


        state.uberReqs = state.originPokemon.map(function(obj){
            return { requestId: obj.id, dlat: obj.position.lat, dlng : obj.position.lng };
        });
        console.log('GET /rank');
        state.consulClient.catalog.service.nodes('pokedash-uberestimate', lookupPokeDashUberestimate.bind(undefined, state));
    }
});



function getPokeDashUberPoke(state)
{

    request({
            method: 'GET',
            url: state.pokeDashUberestimateUrl,
            qs: {  slat: state.slat, slng: state.slng },
            body: state.uberRequestInfo,
            json: true
        },
        function (error, response, body) {
            if (!error && response.statusCode == 200) {
                var uberest = body;
                state.originResponse.status(200).json({uberPrice: uberest[0].price, uberTime: uberest[0].time});
            } else
                state.originResponse.status(500);
        });

}



function lookupPokeDashUberPoke(state, err, result) {
    state.consulClient.catalog.service.nodes('pokedash-uberestimate', function (err, result) {
        if (!err) {
            if (result.length > 0) {
                state.pokeDashUberestimateUrl = 'http://' + result[0].Address + ':' + result[0].ServicePort + '/pokedash-uberestimate';
                console.log("GET /poll: service 'pokedash-uberestimate' available at '" + state.pokeDashUberestimateUrl +"'");
                getPokeDashUberPoke(state);
            }
            else {
                console.log("GET /poll: Consul: service 'pokedash-uberestimtates' not available");
                state.originResponse.status(404).send("Consul: service 'pokedash-uberestimate' not available");
            }
        } else {
            console.log("GET /poll: service 'pokedash-uberestimtates' not available");
            state.originResponse.status(503).send("service 'pokedash-scan' not available");
        }
    });

}

app.get('/tap', function (req, res) {

    req.body.id
    var state = {
        originResponse: res,
        uberRequestInfo : [{requestId:0,
            dlat: req.query.dlat,
            dlng: req.query.dlng}],
        slat: req.query.slat,
        slng: req.query.slng,
        consulClient: consul()

    }
    state.consulClient.catalog.service.nodes('pokedash-uberestimate', lookupPokeDashUberPoke.bind(undefined, state));

});



app.listen(3000, function () {
    console.log('PokeDash API Gateway listening on port 3000!');


});