(function() {

    'use strict';

    var validateFields = angular.module('validateFields', []);

    validateFields.service('ValidateFields', [
        function () {
            this.EMAIL_REGEX = /^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$/;
            this.PAS_REGEX = /^([0-9a-zA-Z])$/;
            this.TELEFNO_REGEX = /^[0-9]{9}$/;
            this.TWITTER = /^@?(\w){1,15}$/;
            this.NOMBRE = /^([a-zA-Z]{1,15})$/;

            this.validateEmail = function (str) {
                return (str.match(this.EMAIL_REGEX) != null);
            }

            this.validatePassport = function (str) {
                return (str.match(this.PAS_REGEX) != null);
            }

            this.validateTelefono = function (str) {
                return (str.match(this.TELEFNO_REGEX) != null);
            }

            this.validateTwitter = function (str) {
                return (str.match(this.TWITTER) != null);
            }

            this.validateNombre = function (str) {
                return (str.match(this.NOMBRE) != null);
            }

            this.maskTelefono = function (tel) {
                if (!tel) { return ''; }

                var value = tel.toString().trim().replace(/^\+/, '');

                if (value.match(/[^0-9]/)) {
                    return tel;
                }

                var country, city, number;

                switch (value.length) {
                    case 10: // +1PPP####### -> C (PPP) ###-####
                        country = 1;
                        city = value.slice(0, 3);
                        number = value.slice(3);
                        break;

                    case 11: // +CPPP####### -> CCC (PP) ###-####
                        country = value[0];
                        city = value.slice(1, 4);
                        number = value.slice(4);
                        break;

                    case 12: // +CCCPP####### -> CCC (PP) ###-####
                        country = value.slice(0, 3);
                        city = value.slice(3, 5);
                        number = value.slice(5);
                        break;

                    default:
                        return tel;
                }

                if (country == 1) {
                    country = "";
                }

                number = number.slice(0, 3) + '-' + number.slice(3);

                return (country + " (" + city + ") " + number).trim();
            }
        }
    ]);


})();