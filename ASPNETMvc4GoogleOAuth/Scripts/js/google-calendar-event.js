angular.module('gevent', ['ngRoute', 'ngResource', 'gevent.service', 'gevent.controller'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
        .when('/', {
            templateUrl: '/Scripts/js/list.html',
            controller: 'listCtrl',
            resolve: {
                _events: ['eventsLoader', function (eventsLoader) { return eventsLoader(); }]
            }
        })
        .when('/add', {
            templateUrl: '/Scripts/js/add.html',
            controller: 'addCtrl',
            resolve: {
                googleCalendars: ['googleCalendarLoader', function (googleCalendarLoader) { return googleCalendarLoader(); }]
            }
        })
        .when('/edit/:id', {
            templateUrl: '/Scripts/js/edit.html',
            controller: 'editCtrl',
            resolve: {
                _event: ['$route', 'eventLoader', function ($route, eventLoader) {                    
                    return eventLoader($route.current.params.id);
                }]
            }
        })
        .otherwise({ redirectTo: '/' });
    }]);

// service 
angular.module('gevent.service', [])
    .factory('GEVENT', ['$resource', function ($resource) {
        var resetUrl = '/Event';
        var resource = $resource(resetUrl + '/:action',
          {

          }, {
              getAll: {
                  method: 'POST',
                  params: {
                      action: "GetAll"
                  },
                  isArray: true
              },
              fetchGoogleCalendar: {
                  method: 'POST',
                  params: {
                      action: 'FetchGoogleCalendar'
                  },
                  isArray: true
              },
              create: {
                  method: 'POST',
                  params: {
                      action: 'Create'
                  }
              },
              getEventByID: {
                  method: 'POST',
                  params: {
                      action: 'GetEventByID'
                  }
              },
              updateEventByID: {
                  method: 'POST',
                  params: {
                      action: 'UpdateEventByID'
                  }
              },
              deleteEventByID: {
                  method: 'POST',
                  params: {
                      action: 'DeleteEventByID'
                  }
              }
          });

        return resource;
    }])
    .factory('eventsLoader', ['$q', 'GEVENT', function ($q, GEVENT) {
        return function () {
            var delay = $q.defer();
            GEVENT.getAll(function (data) {
                delay.resolve(data);
            }, function () {
                delay.reject('Unable to fetch database gevent list');
            });
            return delay.promise;
        };
    }])
    .factory('googleCalendarLoader', ['$q', 'GEVENT', function ($q, GEVENT) {
        return function () {
            var delay = $q.defer();
            GEVENT.fetchGoogleCalendar(function (data) {
                delay.resolve(data);
            }, function () {
                delay.reject('Unable to fetch google calendar list');
            });
            return delay.promise;
        };
    }])
    .factory('eventLoader', ['$q', 'GEVENT', function ($q, GEVENT) {
        return function (id) {
            var delay = $q.defer();            
            GEVENT.getEventByID({ 'id': id }, function (data) {
                delay.resolve(data);
            }, function () {
                delay.reject('Unable to fetch database event by id');
            });
            return delay.promise;
        };
    }]);

// controller
angular.module('gevent.controller', [])
    .controller('listCtrl', ['$scope', '_events', 'GEVENT', function ($scope, _events, GEVENT) {
        $scope.events = _events;

        $scope.delete = function (g) {
            GEVENT.deleteEventByID({
                guid: g.guid,
                calendarId: g.calendarId,
                id: g.Id
            }, function (status) {
                if (status) {
                    GEVENT.getAll(function (data) {
                        $scope.events = data;
                    }, function () {
                        
                    });
                }
            }, function (error) {
                console.error(error);
            });
        };
    }])
    .controller('addCtrl', ['$scope', '$location', 'googleCalendars', 'GEVENT', function ($scope, $location, googleCalendars, GEVENT) {
        // init
        $scope.template = '/Scripts/js/template/_edit.html?t=' + +new Date();
        $scope.calendars = (function (calendar) {
            angular.forEach(calendar, function (item, index) {
                item.selected = false;
            });
            return calendar;
        })(googleCalendars);

        // event post request body
        $scope.event = {
            calendarId: '',
            requestBody: {
                start: {
                    date: moment().format('YYYY-MM-DD')
                },
                end: {
                    date: moment().add(3, 'd').format('YYYY-MM-DD')
                }
            }
        };

        $scope.setSelect = function (Id) {
            angular.forEach($scope.calendars, function (item, index) {
                if (item.Id == Id)
                    item.selected = true;
                else
                    item.selected = false;
            });
            $scope.event.calendarId = Id;
        };5

        $scope.create = function (r) {           
            r.requestBody = JSON.stringify(r.requestBody);
      
            GEVENT.create({
                calendarId: r.calendarId,
                requestBody: r.requestBody
            }, function (data) {
                $location.path('/');
            }, function (error) {
                console.log(error);
            });
        };
    }])
    .controller('editCtrl', ['$scope', '$location', '_event', 'GEVENT', function ($scope, $location, _event, GEVENT) {
        $scope.template = '/Scripts/js/template/_edit.html?t='+ +new Date();

        $scope.event = {
            guid:_event.guid,
            calendarId: _event.calendarId,
            id:_event.Id,
            requestBody: {
                start: {
                    date:moment(_event.start).format('YYYY-MM-DD')
                },
                end:{
                    date: moment(_event.end).format('YYYY-MM-DD')
                },
                summary : _event.summary,
                description: _event.description                
            }
        };

        $scope.update = function (r) {
            r.requestBody = JSON.stringify(r.requestBody);

            GEVENT.updateEventByID({
                calendarId: r.calendarId,
                guid: r.guid,
                id: r.id,
                requestBody: r.requestBody
            }, function (data) {
                $location.path('/');
            }, function (error) {
                console.error(error);
            });
        };
    }]);