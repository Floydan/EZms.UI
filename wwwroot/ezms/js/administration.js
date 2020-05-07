(function ($) {
    'use strict';

    window.administration = function () {
        var self = this;
        var _lockUrl = null;
        var _unlockUrl = null;
        var _deleteUrl = null;
        var _antiForgeryToken = null;


        function getAntiForgeryToken(token) {
            token = $(token).val();
            return token;
        }

        self.init = function (lockUrl, unlockUrl, deleteUrl, token) {
            _lockUrl = lockUrl;
            _unlockUrl = unlockUrl;
            _deleteUrl = deleteUrl;

            _antiForgeryToken = getAntiForgeryToken(token);

            $('.js-account-lock').on('click', self.lockAccount);
            $('.js-account-unlock').on('click', self.unlockAccount);
            $('.js-account-delete').on('click', self.deleteAccount);

            return self;
        };

        self.lockAccount = function (e) {
            e.preventDefault();

            $.ajax(_lockUrl, {
                dataType: 'json',
                method: 'GET',
                contentType: 'application/json; charset=utf-8',
                headers: {
                    RequestVerificationToken: getAntiForgeryToken()
                }
            })
            .done(function (response) {
                window.location.href = response.redirectUrl;
            })
            .fail(function (err) {
                console.log(err);
            });

            return false;
        };

        self.unlockAccount = function (e) {
            e.preventDefault();

            $.ajax(_unlockUrl, {
                dataType: 'json',
                method: 'GET',
                contentType: 'application/json; charset=utf-8',
                headers: {
                    RequestVerificationToken: getAntiForgeryToken()
                }
            })
            .done(function (response) {
                window.location.href = response.redirectUrl;
            })
            .fail(function (err) {
                console.log(err);
            });

            return false;
        };

        self.deleteAccount = function (e) {
            e.preventDefault();

            if (!confirm('Are you absolutely sure you want to do this?'))
                return false;

            $.ajax(_deleteUrl, {
                dataType: 'json',
                method: 'GET',
                contentType: 'application/json; charset=utf-8',
                headers: {
                    RequestVerificationToken: getAntiForgeryToken()
                }
            })
            .done(function (response) {
                window.location.href = response.redirectUrl;
            })
            .fail(function (err) {
                console.log(err);
            });

            return false;
        };

        return self;
    };
})(jQuery);