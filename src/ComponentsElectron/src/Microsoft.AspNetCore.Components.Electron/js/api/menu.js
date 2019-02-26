"use strict";
exports.__esModule = true;
var electron_1 = require("electron");
var contextMenuItems = [];
module.exports = function (socket) {
    socket.on('menu-setContextMenu', function (browserWindowId, menuItems) {
        var menu = electron_1.Menu.buildFromTemplate(menuItems);
        addContextMenuItemClickConnector(menu.items, browserWindowId, function (id, browserWindowId) {
            socket.emit("contextMenuItemClicked", [id, browserWindowId]);
        });

        // BUGFIX: When changing the context menu for a given browser window,
        // remove the previous state
        var existingIndex = contextMenuItems.findIndex(val => val.browserWindowId === browserWindowId);
        if (existingIndex >= 0) {
            contextMenuItems.splice(existingIndex, 1);
        }

        contextMenuItems.push({
            menu: menu,
            browserWindowId: browserWindowId
        });
    });
    function addContextMenuItemClickConnector(menuItems, browserWindowId, callback) {
        menuItems.forEach(function (item) {
            if (item.submenu && item.submenu.items.length > 0) {
                addContextMenuItemClickConnector(item.submenu.items, browserWindowId, callback);
            }
            if ("id" in item && item.id) {
                item.click = function () { callback(item.id, browserWindowId); };
            }
        });
    }
    socket.on('menu-contextMenuPopup', function (browserWindowId) {
        contextMenuItems.forEach(function (x) {
            if (x.browserWindowId === browserWindowId) {
                var browserWindow = electron_1.BrowserWindow.fromId(browserWindowId);
                x.menu.popup(browserWindow);
            }
        });
    });
    socket.on('menu-setApplicationMenu', function (menuItems) {
        var menu = electron_1.Menu.buildFromTemplate(menuItems);
        addMenuItemClickConnector(menu.items, function (id) {
            socket.emit("menuItemClicked", id);
        });
        electron_1.Menu.setApplicationMenu(menu);
    });
    function addMenuItemClickConnector(menuItems, callback) {
        menuItems.forEach(function (item) {
            if (item.submenu && item.submenu.items.length > 0) {
                addMenuItemClickConnector(item.submenu.items, callback);
            }
            if ("id" in item && item.id) {
                item.click = function () { callback(item.id); };
            }
        });
    }
};
//# sourceMappingURL=menu.js.map