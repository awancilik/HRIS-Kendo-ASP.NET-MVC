/// <reference path="jquery.signalR-2.1.2.js" />

var notification = {
    hub: $.connection.notificationHub,
    populateExistingNotification: function() {
        var notificationContent = $("#notification_container").children();
        var existingNotifications = new Array();
        for (var i = 0; i < notificationContent.length; i++) {
            var existingNotification = {
                notificationId: notificationContent[i].id,
                notificationMessage: $(notificationContent[i]).text()
            };
            existingNotifications.push(existingNotification);
        }
        return existingNotifications;
    },
    isNotificationExist: function (obj) {
        var existingNotifications = notification.populateExistingNotification();
        for (var i = 0; i < existingNotifications.length; i++) {
            if (existingNotifications[i].notificationId === obj.id.toString()) {
                return true;
            }
        }
        return false;
    },
    addNewNotificationElement: function (result) {
        var unreadNotifications = $.parseJSON(result);
        for (var i = 0; i < unreadNotifications.length; i++) {
            if (notification.isNotificationExist(unreadNotifications[i]) == false) {
                if ($("#notification_container").children().length >= 7) {
                    $("#notification_container div:last-child").fadeOut(500, function () {
                        this.remove();
                    });
                }
                $("#notification_container").prepend("<div id='" + unreadNotifications[i].id + "' class='notification_box fade_in'>" + unreadNotifications[i].message + "</div>");
            }
        }
    }
};


$(function () {
    notification.hub.client.getNotifications = notification.addNewNotificationElement;

    $.connection.hub.start();
});