/**
 * AngularJS service to validate spanish document id.
 * Returns the type of document and checks its validity.
 *
 * Usage:
 *     angular
 *     .module('myApp', [ 'validate-spanish-id' ])
 *     .controller('myController', function(ValidateSpanishID){
 *         ValidateSpanishID.validate( str );
 *     })
 *
 *
 *     > ValidateSpanishID.validate( '12345678Z' );
 *     // { type: 'dni', valid: true }
 *
 *     > ValidateSpanishID.validate( 'B83375575' );
 *     // { type: 'cif', valid: false }
 *
 * The algorithm is adapted from other solutions found at:
 * - https://gist.github.com/afgomez/5691823
 * - http://www.compartecodigo.com/javascript/validar-nif-cif-nie-segun-ley-vigente-31.html
 * - http://es.wikipedia.org/wiki/C%C3%B3digo_de_identificaci%C3%B3n_fiscal
 */
(function() {

    'use strict';

    var validateSpanishId = angular.module('validateSpanishId', []);

    validateSpanishId.service('ValidateSpanishID', [function () {
        this.DNI_REGEX = /^(\d{8})([A-Z])$/;
        this.CIF_REGEX = /^([ABCDEFGHJKLMNPQRSUVW])(\d{7})([0-9A-J])$/;
        this.NIE_REGEX = /^[XYZ]\d{7,8}[A-Z]$/;

        this.validate = function (str) {
            // If no valid value passed
            if (!str) {
                str = '';
            }
            // Ensure upcase and remove whitespace
            str = str.toUpperCase().replace(/\s/, '');

            var valid = false;
            var type = this.idType(str);

            switch (type) {
                case 'dni':
                    valid = this.validateDNI(str);
                    break;

                case 'nie':
                    valid = this.validateNIE(str);
                    break;

                case 'cif':
                    valid = this.validateCIF(str);
                    break;
            }

            return {
                type: type,
                valid: valid
            };
        };

        this.idType = function (str) {
            if (str.match(this.DNI_REGEX)) {
                return 'dni';
            }
            else if (str.match(this.CIF_REGEX)) {
                return 'cif';
            }
            else if (str.match(this.NIE_REGEX)) {
                return 'nie';
            }
            else {
                return '';
            }
        };

        this.validateDNI = function (dni) {
            var dni_letters = "TRWAGMYFPDXBNJZSQVHLCKE";
            var letter = dni_letters.charAt(parseInt(dni, 10) % 23);

            return letter == dni.charAt(8);
        };

        this.validateNIE = function (nie) {
            // Change the initial letter for the corresponding number and validate as DNI
            var nie_prefix = nie.charAt(0);

            switch (nie_prefix) {
                case 'X':
                    nie_prefix = 0;
                    break;

                case 'Y':
                    nie_prefix = 1;
                    break;

                case 'Z':
                    nie_prefix = 2;
                    break;
            }
            return this.validateDNI(nie_prefix + nie.substr(1));
        };

        this.validateCIF = function (cif) {
            var match = cif.match(this.CIF_REGEX);
            var letter = match[1],
                number = match[2],
                control = match[3];

            var even_sum = 0;
            var odd_sum = 0;
            var n;

            for (var i = 0; i < number.length; i++) {
                n = parseInt(number[i], 10);

                // Odd positions (Even index equals to odd position. i=0 equals first position)
                if (i % 2 === 0) {
                    // Odd positions are multiplied first.
                    n *= 2;

                    // If the multiplication is bigger than 10 we need to adjust
                    odd_sum += n < 10 ? n : n - 9;

                    // Even positions
                    // Just sum them
                } else {
                    even_sum += n;
                }
            }

            var control_digit = (10 - (even_sum + odd_sum).toString().substr(-1));
            var control_letter = 'JABCDEFGHI'.substr(control_digit, 1);

            // Control must be a digit
            if (letter.match(/[ABEH]/)) {
                return control == control_digit;

                // Control must be a letter
            } else if (letter.match(/[KPQS]/)) {
                return control == control_letter;

                // Can be either
            } else {
                return control == control_digit || control == control_letter;
            }
        };
    }])
    
})();