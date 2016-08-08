document.addEventListener('DOMContentLoaded', function () {
  if (document.querySelectorAll('#map').length > 0)
  {
    if (document.querySelector('html').lang) {
      lang = document.querySelector('html').lang;
    }
    else {
      lang = 'en';
    }
    var js_file = document.createElement('script');
    js_file.type = 'text/javascript';
    js_file.src = 'https://maps.googleapis.com/maps/api/js?key=AIzaSyCRe6Arpwj_h4coIFoxYV6XyxrDSw--WJU&callback=initMap&signed_in=true&language=' + lang;
    document.getElementsByTagName('head')[0].appendChild(js_file);
  }
});

function initMap() {
  var map;
  var pokeHash;
  var position;
  var positionMarker;
  var currentPokemon;


/*******************************************************************************
-------------- POKEMARKER ------------------------------------------------------
*******************************************************************************/
  PokeMarker = function(data) {
    this.position = data.position;
    this.image_ = null;
    this.setMap(map);
    this.ready = false;
    this.div_ = null;
    this.id = data.id;
    this.name = data.name;
    this.number = data.number;
    this.expires = data.expires;
    this.rank = null;
    this.delete = false;
  }

  PokeMarker.prototype = new google.maps.OverlayView();
  PokeMarker.prototype.onAdd = function() {
    var div = document.createElement('div');
    var srcImage = 'url("resources/images/';
    var number = "" + this.number;
    var pad = '000';
    srcImage += pad.substring(0,pad.length - number.length) + number;
    // [Colter][2016.08.01] This turns '1' into '001', etc.
    srcImage += '.png")';
    div.style['background-image'] = srcImage;
    div.className ='poke-marker';
    this.div_ = div;
    var panes = this.getPanes();
    panes.overlayMouseTarget.appendChild(div);
    var pokemon = this;
    google.maps.event.addDomListener(div, 'click', function() {
      event.stopPropagation();
      tap(pokemon.id);
    });
    this.ready = true;
  };
  PokeMarker.prototype.draw = function() {
    var overlayProjection = this.getProjection();
    var latlng = new google.maps.LatLng(this.position.lat, this.position.lng);
    var point = overlayProjection.fromLatLngToDivPixel(latlng);
    var div = this.div_;
    div.style.left = point.x + 'px';
    div.style.top = point.y + 'px';
  }
  PokeMarker.prototype.onRemove = function() {
    this.div_.parentNode.removeChild(this.div_);
    this.div_ = null;
  }


/*******************************************************************************
-------------- UTILITIES -------------------------------------------------------
*******************************************************************************/
  function fail(message) {
    console.error(message);
    return 1;
  };

  function getTime() {
    return new Date().getTime() / 1000;
  }


  function setInfoBoxSize(status) {
    var container = document.getElementById('container');
    var mapContainer = document.getElementById('map-container');
    container.className = 'info-' + status;

    if (status == 'on') {
      if(pokeHash[currentPokemon]) {
        map.panTo(pokeHash[currentPokemon].position)
      }
    }
  }

  function getInfoBoxSize() {
    var container = document.getElementById('container');
    return container.className.substring(5);
  }


/*******************************************************************************
-------------- RANK ------------------------------------------------------------
*******************************************************************************/
function renderRank(id) {
  var pokemon = pokeHash[id];
  if (pokemon) {
    var div = pokemon.div_;
    if (!div) return fail("pokemon doesn't have div");
    if (div.classList.contains('dead')) return;
    div.classList.remove('rank-1');
    div.classList.remove('rank-3');
    div.classList.remove('rank-4');
    div.classList.remove('rank-2');
    div.classList.remove('rank-5');
    div.classList.remove('rank-null');
    if (pokemon.rank == null) {
      //div.classList.add('rank-null');
    }
    else if (pokemon.rank < 5) {
      div.classList.add('rank-1');
    }
    else if (pokemon.rank < 10) {
      div.classList.add('rank-2');
    }
    else if (pokemon.rank < 15) {
      div.classList.add('rank-3');
    }
    else if (pokemon.rank < 20) {
      div.classList.add('rank-4');
    }
    else {
      div.classList.add('rank-5');
    }
  }
}

  function serverRank(pokemon, lat, lng) {
    var request = new XMLHttpRequest();
    request.open('POST', 'http://c04dghhack5.ds.ad.adp.com:3000/rank?slat='+lat+'&slng='+lng, true);
	request.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
	
    request.send(JSON.stringify(pokemon));
	request.onreadystatechange = function () {
		if (request.status == 200) {
			rankCallback(JSON.parse(request.responseText));
		}
		else {
			currentlyRanking =false;
		}
	};
  }

  var currentlyRanking = false;
  
  function rankCallback(ranking) {
    
	var rank=0;
    var string = "";
	for (var id in pokeHash) {
		if (pokeHash[id]) {
			pokeHash[id].rank = null;
		}
	}
	
    for (var i=0; i < ranking.length; i++) {
      if (pokeHash[ranking[i]]) {
        pokeHash[ranking[i]].rank = rank++;
        string += ranking[i] + " "
      }
      else {
        fail('ranking failed because pokeHash[' + ranking[i] + '] did not exist');
      }
    }
    currentlyRanking = false;
  }

  function rank() {
    if (currentlyRanking) return;
    currentlyRanking = true;
    var pokeArray = [];
    var counter = 0;
    for (var id in pokeHash) {
      var pokemon;
      if (pokeHash[id]) {
        pokemon = pokeHash[id];
        var time = getTime();
        pokeArray.push({
          id: id,
          name: pokemon.name,
          number: pokemon.number,
          position: pokemon.position,
          ttl: pokemon.expires - time,
        })
        counter++;
      }
    }
    serverRank(pokeArray, position.lat, position.lng);
  }


/*******************************************************************************
-------------- POLL ------------------------------------------------------------
*******************************************************************************/
  function updatePokemon(data, time) {
    var id = data.id;
    if (id == null) return fail('Pokemon #' + i + ' does not have id');
    if (pokeHash[id] == null) {
      if (!data.number) return fail('Pokemon id #' + data.number + ' does not have number');
      if (!data.position) return fail('Pokemon id #' + data.id + ' does not have position');
      if (!data.name) return fail('Pokemon id #' + data.id + ' does not have name');
      if (!data.ttl) return fail('Pokemon id #' + data.id + ' does not have time to live');
      pokeHash[id] = new PokeMarker({
        id: id,
        name: data.name,
        number: data.number,
        ttl: data.ttl,
        expires: time + data.ttl,
        position: {lat: data.position.lat, lng: data.position.lng},
        marker: null
      });
    }

  }

  var tempFakeIdCount = 0 // TODO [Colter][2016.08.04] Make sure to get rid of this
  function serverPoll(lat, lng) {
    var request = new XMLHttpRequest();
    request.open('GET', 'http://c04dghhack5.ds.ad.adp.com:3000/poll?lat='+lat+'&lng='+lng, true);
	
    request.send();
	request.onreadystatechange = function () {
		if (request.status == 200) {
			result = JSON.parse(request.response);
			pollCallback(result);
		}
	};
  }
  
  function pollCallback(data) {
    var time = getTime();
    var pokemon
    while (pokemon = data.pop()) {
      updatePokemon(pokemon, time);
    }
  }

  function poll() {
    var data = serverPoll(position.lat, position.lng);
  };


/*******************************************************************************
-------------- TAP -------------------------------------------------------------
*******************************************************************************/
  function updateTTL(){
    var timeToLiveField = document.getElementById('info-time-to-live');
    if (currentPokemon == null) return fail('currentPokemon is null');
    var pokemon = pokeHash[currentPokemon];
    if (!pokemon) {
      currentPokemon = null;
      return false;
    };
    var ttl = Math.round(pokemon.expires - getTime());
    timeToLiveField.innerHTML = ttl + " seconds left";
    if (ttl <= 0) {
      setInfoBoxSize('off');
    }
  }

  function serverTap(slat, slng, dlat, dlng, pokemon) {
    var request = new XMLHttpRequest();
    request.open('GET', 'http://c04dghhack5.ds.ad.adp.com:3000/tap?slat='+slat+'&slng='+slng+'&dlat='+dlat+'&dlng='+dlng, true);
	
    request.send(null);
	request.onreadystatechange = function () {
		if (request.status == 200) {
			tapCallback(JSON.parse(request.responseText), pokemon);
		}
	};
  }
  
  function tapCallback(result, pokemon) {
    var infoStatus = document.getElementById('container').className
    var infobox = document.getElementById('info');
    var nameField = document.getElementById('info-name');
    var numberField = document.getElementById('info-number');
    var rankingField = document.getElementById('info-ranking');
    var timeToLiveField = document.getElementById('info-time-to-live');
    var uberField = document.getElementById('info-uber');
    var uberCostField = document.getElementById('uber-cost');
    var uberTimeField = document.getElementById('uber-time');
    var uberButton = document.getElementById('uber-button');
    nameField.innerHTML = pokemon.name;
    numberField.innerHTML = '#' + pokemon.number;
    timeToLiveField.innerHTML = Math.round(pokemon.expires - getTime()) + " seconds left";
    var ttlCounter = setInterval(updateTTL, 1000);
    uberCostField.innerHTML = result.uberPrice;
    uberTimeField.innerHTML = Math.round(result.uberTime / 60) + ' min away';
    var request = 'https://m.uber.com/ul'
    request+='?pickup[latitude]=' + position.lat;
    request+='&pickup[longitude]=' + position.lng;
    request+='&dropoff[latitude]=' + pokemon.position.lat;
    request+='&dropoff[longitude]=' + pokemon.position.lng;

    uberButton.href = request;
    setInfoBoxSize('on');
    document.getElementById('map-container').addEventListener(
      'click',
      function() {
        setInfoBoxSize('off');
        clearInterval(ttlCounter);
      },
      {once: true}
    );
  }

  function tap(id) {
    currentPokemon = id;
    var pokemon = pokeHash[id];
    if (!pokemon) return fail('Tapped pokemon does not exist');
    serverTap(position.lat, position.lng, pokemon.position.lat, pokemon.position.lng, pokemon);
  };


/*******************************************************************************
-------------- PROCESS ---------------------------------------------------------
*******************************************************************************/
  function processPokemon(id) {
  
    if (pokeHash[id] && pokeHash[id].ready) {
      var pokemon = pokeHash[id];
      var div = pokemon.div_;
      if (div) {
        if (pokemon.delete) {
          pokemon.setMap(null);
          pokeHash[id] = null;
          delete pokeHash[id];
        }
        else {
          if (!div.classList.contains('active') && !div.classList.contains('dead')) {
            div.classList.add('active');
          }
          else if (pokemon.expires < getTime() && !div.classList.contains('dead')) {
            div.classList.remove('active');
            div.classList.remove('rank-1');
            div.classList.remove('rank-2');
            div.classList.remove('rank-3');
            div.classList.remove('rank-4');
            div.classList.remove('rank-5');
            div.classList.remove('rank-null');
            div.classList.add('dead');
            setTimeout(function() {pokemon.delete = true}, 1000);
            pokemon.delete = true;
          }
          renderRank(id);
        }
      }
    }
  }

  function pokeProcess() {
    for (var id in pokeHash) {
      processPokemon(id);
    }
  }


/*******************************************************************************
-------------- POSITION --------------------------------------------------------
*******************************************************************************/
  function getPosition() {
    navigator.geolocation.getCurrentPosition(function(result) {
      position = {lat: result.coords.latitude, lng: result.coords.longitude};
      if (positionMarker == null) {
        positionMarker = new google.maps.Marker({
          position: position,
          icon: {
            path: google.maps.SymbolPath.CIRCLE,
            scale: 10,
            fillOpacity: 1.0,
            fillColor: '#00b2a9',
            strokeColor: 'white',
            strokeWeight: 3
          },
          map: map
        });

        positionMarker.setMap(map);
        map.setCenter(position);
      }
      else {
        positionMarker.setPosition(position);
      }
    }, function(msg) {
      fail(msg);
    });
  };


/*******************************************************************************
-------------- EVENT -----------------------------------------------------------
*******************************************************************************/
  function infoboxClick() {
    var infobox = document.getElementById('info');
    if (getInfoBoxSize() == 'on') {
      setInfoBoxSize('full');
    }
    else if (getInfoBoxSize() == 'full') {
      setInfoBoxSize('on');
    }
  }

  function uberTitleClick() {
    var infoIcon = document.getElementById('info-icon');
    if (infoIcon.onClick) {
      infoIcon.onClick = function () {
        location.href = "https://m.uber.com/ul/";
      }
    }
  }

  if (loading.className == 'down') {
    var loadingName = document.getElementById('info-loadingName');
    loadingName.innerHTML = 'Finding Pokemon...';
  }
/*******************************************************************************
-------------- INIT ------------------------------------------------------------
*******************************************************************************/
  position = {lat: 45.5043037,lng: -122.6798189};

  map = new google.maps.Map(document.getElementById('map'), {
    center: position,
    zoom: 16,
    mapTypeId: 'roadmap',
    disableDefaultUI: true
  });

  pokeHash = {};

  setTimeout(function() {loading.className = 'up'}, 10000);

  var infoName = document.getElementById('info-name');
  infoName.addEventListener('click', infoboxClick);

  var infoIcon = document.getElementById('info-icon');
  if (infoIcon) {
    infoIcon.addEventListener('click', uberTitleClick);
  }

  getPosition();
  poll();
  rank();
  var positionInterval = setInterval(getPosition, 5000);
  var pollInterval = setInterval(poll, 10000);
  var rankInterval = setInterval(rank, 5000);
  var processInterval = setInterval(pokeProcess, 1000);
};
