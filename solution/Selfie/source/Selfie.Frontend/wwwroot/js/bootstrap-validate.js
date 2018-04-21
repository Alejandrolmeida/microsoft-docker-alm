(function () {

    'use strict';

    var validateBootstrap = angular.module('validateBootstrap', []);

    validateBootstrap.service('ValidateBootstrap', [function () {
        this.Init = function (FormID) {

            $(FormID)
                .formValidation({
                    framework: 'bootstrap',
                    icon: {
                        valid: 'glyphicon glyphicon-ok',
                        invalid: 'glyphicon glyphicon-remove',
                        validating: 'glyphicon glyphicon-refresh'
                    },
                    fields: {
                        documento: {
                            validators: {
                                notEmpty: {
                                    message: 'The document is required and cannot be empty'
                                }
                            }
                        },
                        email: {
                            enabled: false,
                            validators: {
                                notEmpty: {
                                    message: 'The email is required and cannot be empty'
                                }
                            }
                        },
                        telefono: {
                            enabled: false,
                            validators: {
                                notEmpty: {
                                    message: 'The phone is required and cannot be empty'
                                },
                                identical: {
                                    field: 'password',
                                    message: 'The password and its confirm must be the same'
                                }
                            }
                        }
                    }
                })
                // Enable the password/confirm password validators if the password is not empty
                .on('keyup', '[name="telefono"]', function () {
                    var isEmpty = $(this).val() == '';
                    
                });

        }
    }])
})();