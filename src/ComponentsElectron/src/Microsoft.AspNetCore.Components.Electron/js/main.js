const { app } = require('electron');
const process = require('process');

const port = process.argv[2];
app.on('ready', () => {
  // TODO: Don't use TCP as the transport:
  // [1] It's messy that it displays the Windows "can this process listen?" dialog
  // [2] It's not doing anything to prevent other processes from connecting and then
  //     running operations in the security context of the Electron process
  io = require('socket.io')(port);
  console.log('socket.io is listening on port ' + port);

  io.on('connection', (socket) => {
    console.log('.NET Core Application connected...');

    require('./api/app')(socket, app);
    require('./api/browserWindows')(socket);
    require('./api/ipc')(socket);
    require('./api/menu')(socket);
    require('./api/dialog')(socket);
    require('./api/notification')(socket);
    require('./api/tray')(socket);
    require('./api/webContents')(socket);
    require('./api/globalShortcut')(socket);
    require('./api/shell')(socket);
    require('./api/screen')(socket);
    require('./api/clipboard')(socket);
  });
});
