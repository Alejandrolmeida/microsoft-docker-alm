var app = angular.module('selfie', ['azureBlobUpload', 'validateSpanishId', 'validateFields', 'navAnimation']);

//app.config(function ($httpProvider) {
//    //Enable cross domain calls
//    $httpProvider.defaults.useXDomain = true;
//});

app.directive('fileModel', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var model = $parse(attrs.fileModel);
            var modelSetter = model.assign;

            element.bind('change', function () {
                scope.$apply(function () {                    
                    if (!element[0].files) {
                        if (scope.funcValidarFichero(element.context.id, element[0].value)) {
                            modelSetter(scope, element[0].value);
                        } else {
                            modelSetter(scope, null);
                        };
                    } else {
                        if (scope.funcValidarFichero(element.context.id, element[0].files[0])) {
                            modelSetter(scope, element[0].files[0]);
                        } else {
                            modelSetter(scope, null);
                        };
                    }
                });
            });
        }
    };
}]);

app.directive('ncgRequestVerificationToken', ['$http', function ($http) {
    return function (scope, element, attrs) {
        $http.defaults.headers.common['RequestVerificationToken'] = attrs.ncgRequestVerificationToken || "No se ha conseguido Verificar";
    };
}]);

app.controller('SelfieStyle', ['NavAnimation', function (NavAnimation) {
    NavAnimation.navSmoothScroll();
    NavAnimation.navScroll();
}]);

app.controller('SelfieController', ['$scope', '$http', 'ValidateSpanishID', 'ValidateFields', 'azureBlob', function ($scope, $http, ValidateSpanishID, ValidateFields, azureBlob) {

    $scope.master = {
        UserID: "",
        ValidID: { type: '', valid: false },
        email: "",
        ValidEmail: true,
        telefono: "",
        ValidTelefono: true,
        uriForm: "",
        TableToken: "",
        uriFile1a: "",
        uriFile1b: "",
        uriFile1_result: false,
        uriFile2a: "",
        uriFile2b: "",
        uriFile2_result: false,
        browser: ""
    };

    $scope.resultID = {
        value: "",// "has-success has-feedback", //has-error 
        icon: ""//"ok" //remove
    };
    $scope.resultEmail = {
        value: "",// "has-success has-feedback", //has-error 
        icon: ""//"ok" //remove
    };
    $scope.resultTelefono = {
        value: "",// "has-success has-feedback", //has-error 
        icon: ""//"ok" //remove
    };
    $scope.funcFueraServicio = function () {
            $("#FueraServicio").modal();
    };
    $scope.funcGetSettings = function () {
        $http.get('/config')
            .then(function (response){
                $scope.uriForm = response.data.backend;
            });
    };

    $scope.funcGetSettings();
    $scope.funcFueraServicio();
    $scope.tamanno = 10485760;
    $scope.acepta = false;
    $scope.imagenEstado1 = "glyphicon-plus-sign";
    $scope.imagenEstado2 = "glyphicon-plus-sign";
    $scope.clausula = "Todos tus datos seran eliminados una vez finalice el evento Global Azure Bootcamp 2018 y no serán en nigun caso cedidos a terceros ni usados con otros fines que no sean los propios de exhibir o realizar demostraciones de la solución durante la ejecución del evento.";
    $scope.mensageError = "";
    $scope.mensajeFinSubida = "";
    $scope.mostrarCapaCerrar = false;
    $scope.fichero1Subido = false;
    $scope.fichero2Subido = false;

    $scope.reset = function () {
        var form = $scope.master;
        $scope.user = angular.copy(form);
    };
    $scope.funcCompruebaSubidas = function () {
        var contSubida = 0;
        if ($scope.fichero1Subido) contSubida++;
        //if ($scope.fichero2Subido) contSubida++;

        switch (contSubida) {
            case 0:
                $scope.mensajeFinSubida = "Enviados 0 ficheros con éxito";
                break;
            case 1:
                $scope.mensajeFinSubida = "Enviado 1 fichero con exito";
                break;
            case 2:
                $scope.mensajeFinSubida = "Carga completada correctamente";
                $scope.mostrarCapaCerrar = true;
                $scope.funcLimpiarForm();
                break;
        }
    };
    $scope.funcValidaID = function (str,validateErrors) {
        //$scope.user.ValidID = ValidateSpanishID.validate(str);        
        //if ($scope.user.ValidID.valid) {
        //    $scope.resultID.value = "has-success has-feedback";
        //    $scope.resultID.icon = "ok";
        //} else {
        //    if (validateErrors) {
        //        $scope.resultID.value = "has-error has-feedback";
        //        $scope.resultID.icon = "remove";
        //    }
        //}
        if (str != null && str !== "") {
            $scope.user.ValidID = ValidateFields.validateNombre(str);
            if ($scope.user.ValidID) {
                $scope.resultID.value = "has-success has-feedback";
                $scope.resultID.icon = "ok";
            } else {
                $scope.resultID.value = "has-error has-feedback";
                $scope.resultID.icon = "remove";
            }
        } else {
            $scope.user.ValidID = true;
            $scope.resultID.value = "";
            $scope.resultID.icon = "";
        }
    };
    $scope.funcValidaEmail = function (str) {
        if (str != null && str !== "") {
            $scope.user.ValidEmail = ValidateFields.validateEmail(str);
            if ($scope.user.ValidEmail) {
                $scope.resultEmail.value = "has-success has-feedback";
                $scope.resultEmail.icon = "ok";
            } else {
                $scope.resultEmail.value = "has-error has-feedback";
                $scope.resultEmail.icon = "remove";
            }
        } else {
            $scope.user.ValidEmail = true;
            $scope.resultEmail.value = "";
            $scope.resultEmail.icon = "";
        }
    };
    $scope.funcValidaTelefono = function (str) {
        if (str != null && str != "") {
            $scope.user.ValidTelefono = ValidateFields.validateTelefono(str);
            if ($scope.user.ValidTelefono) {
                $scope.resultTelefono.value = "has-success has-feedback";
                $scope.resultTelefono.icon = "ok";
            } else {
                $scope.resultTelefono.value = "has-error has-feedback";
                $scope.resultTelefono.icon = "remove";
            }
        } else {
            $scope.user.ValidTelefono = true;
            $scope.resultTelefono.value = "";
            $scope.resultTelefono.icon = "";
        }
    };

    //$scope.formIsValid = function () {
    //    if ((!$scope.user.ValidID) || (!$scope.user.ValidEmail) || (!$scope.myFile1)) return false;
    //    if ((($scope.user.ValidID.type == 'dni') || ($scope.user.ValidID.type == 'nie')) && ($scope.user.ValidEmail) && ($scope.user.ValidTelefono)) {
    //        return true;
    //    } else {
    //        return false;
    //    }
    //}
    $scope.formIsValid = function () {
        if ((!$scope.user.ValidID) || (!$scope.user.ValidEmail) || (!$scope.myFile1)) return false;
        if (($scope.user.ValidID) && ($scope.user.ValidEmail)) {
            return true;
        } else {
            return false;
        }
    }
    
    $scope.funcUploadForm = function (complete) {
        if ($scope.formIsValid()) {
            var form = $scope.user;
            $http.post($scope.uriForm + '/Resources', form)
                .success(function (data, status, headers, config) {
                    $scope.user.TableToken = data.tableToken;

                    $scope.user.uriFile1a = data.uriFile1a;
                    $scope.user.uriFile1b = data.uriFile1b;

                    if (!complete) return true;
                    $scope.mostrarCapaCerrar = false;
                    $("#myModal").modal();

                    $scope.fichero1Subido = false;
                    $scope.uploadFile(1);
                }).
                error(function (data, status, headers, config) {
                    alert('Servidor ocupado. Vuelva a intentarlo pasados unos minutos.');
                });
        }
    };
    $scope.funcMostarClausulas = function(str) {
        if (str == true) {
            $("#clausulaModal").modal();
            //acepta = (!acepta);
        }
    };
    $scope.funcMostarMensajeError = function () {
        if ($scope.mensageError !== "") {
            $("#MensajeError").modal();
        } else {
            $scope.mensageError = "";
        }
    };
    $scope.funcImagenEstado = function (idcontrol,str) {
        if (str == "") {
            if (idcontrol == "fichero1") {
                $scope.imagenEstado1 = 'glyphicon-plus-sign';
            };
        } else {
            if (idcontrol == "fichero1") {
                $scope.imagenEstado1 = "glyphicon-ok-circle";
            };
        }
    };
    $scope.funcResult = function (imageID, result) {
        switch (imageID) {
            case 1:
                $(result).parent().attr('class', 'progress');
                $(result).parent().children('input').attr('class', 'hidden');
                break;
            case 2:
                $(result).parent().attr('class', 'progress');
                $(result).parent().children('input').attr('class', 'hidden');
                break;
        }
    };
    $scope.funcInicializaProgressbar = function(progress) {
        if (!$(progress)) {
            $(progress).textContent = "";
            //aria-valuenow
            $(progress).attr('aria-valuenow', '0');
            //style
            $(progress).attr('style', 'width:0');
        };
    };
    $scope.uploadFile = function (imageID) {
        switch (imageID) {
            case 1:
                $scope.funcInicializaProgressbar('#fileUploadProgress1');
                azureBlob.upload({
                    baseUrl: $scope.user.uriFile1a,
                    sasToken: $scope.user.uriFile1b,
                    file: $scope.myFile1,
                    progress: '#fileUploadProgress1',
                    complete: $scope.funcResult(1, "#fileUploadProgress1"),
                    error: '#fileUploadProgress1',
                    completeAllBlocks: function () {
                        $http.post($scope.uriForm + '/Confirmation', $scope.user).
                            success(function (data, status, headers, config) {
                                $scope.fichero1Subido = true;
                                $scope.funcCompruebaSubidas();
                            }).
                            error(function (data, status, headers, config) {
                                $scope.fichero1Subido = false;
                                $scope.funcMostarMensajeError("Error al subir fichero");
                                $scope.funcCompruebaSubidas();
                            });
                    },
                });
                break;            
        }
    };
    $scope.funcValidarFichero = function (idcontrol,fichero) {
        //10 megas
        $scope.mensageError = "";
        var resultado = true;
        if (fichero.size > parseInt($scope.tamanno)) {
            $scope.mensageError = "El archivo ha superado tamaño de la subida. Maximo " + (parseInt($scope.tamanno) / 1048576) + " Megas";
            resultado = false;
        } else {
            if (fichero.size < 1024) {
                $scope.mensageError = "El archivo seleccionado no es válido. Tamaño minimo 1 (KB)";
                resultado = false;
            };
        };
        if (resultado === true) {
            var tipos = ["image/gif", "image/bmp", "image/jpeg", "image/tiff", "image/tif", "image/jpg", "image/png", "application/pdf"];
            if (tipos.indexOf(fichero.type) < 0) {
                $scope.mensageError = "El archivo seleccionado no corresponde con los formatos permitidos. (gif, jpeg, tiff, png, bmp y pdf's)";
                resultado = false;
            } else {
                resultado = true;
            };
        }
        
        if (resultado==true) {
            $scope.funcImagenEstado(idcontrol, "seleccionado");
        } else {
            $scope.funcMostarMensajeError();
            $scope.funcImagenEstado(idcontrol, "");
        };
        return resultado;
    };
    $scope.funcLimpiarForm = function() {

        $scope.fichero1Subido = false;
        $scope.fichero2Subido = false;
        $scope.mensageError = "";
        $scope.reset();
        $scope.user.ValidID = { type: '', valid: false }
        $scope.funcImagenEstado("fichero1", "");
        $scope.funcImagenEstado("fichero2", "");
        $scope.myFile1 = null;
        $scope.myFile2 = null;
        $scope.resultID = {value: "",icon: ""};
        $scope.resultEmail = {value: "",icon: ""};
        $scope.resultTelefono = { value: "", icon: "" };
        $scope.acepta = false;
    };

    $scope.reset();
    }
]);

app.constant("Modernizr", Modernizr);
app.controller('SupportController', [
    '$scope', '$location', 'Modernizr', function($scope, $location, Modernizr) {
        // check if browser supports HTML5 features
        $scope.funcDetectCapabilitiesBrowers = function() {
            Modernizr.addTest('cssall', true);
            Modernizr.addTest('filereader', true);
            Modernizr.addTest('filesystem', true);
            Modernizr.addTest('stylescoped', true);
            function stringify(obj, minified) {
                var replacer = function (key, value) {
                    return value;
                }
                var args = minified ? [replacer, 2] : [];
                args.unshift(obj);
                return JSON.stringify.apply(JSON, args);
            }

            function ModernizrJson() {
                $scope.user.browser = stringify(Modernizr, false);
            };

            ModernizrJson();
        }
        $scope.funcDetectCapabilitiesBrowers();
        if (window.File && window.FileReader && window.FileList && window.Blob) {
            // Browser is fully supportive.
        } else {
            // Browser not supported. Try normal file upload
            var redireccion = $location.absUrl();
            if (redireccion.indexOf('SupporError') < 0) {
                window.location = $location.absUrl() + 'SupporError';
            }
            //$location.path('SupporError');
        };

}]);