// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const messageLog = document.getElementById('messageLog');
const messageInput = document.getElementById('messageInput');
const sendBtn = document.getElementById('sendBtn');
const connectionStatus = document.getElementById('connectionStatus')

let reconnectInterval; 
let countdown;

function tryConnect() {
    let socket = new WebSocket("ws://localhost:5232/ws");
    
    socket.onopen = function() {
        console.log("Соединение установлено.");
        connectionStatus.textContent = "Соединение установлено."; // Обновляем статус
        
        sendBtn.disabled = false;
        messageInput.disabled = false;
        
        clearInterval(reconnectInterval);
        connectionStatus.textContent = "";

    };

    socket.onmessage = function(event) {
        messageLog.value += event.data + '\n';
    };

    socket.onclose = function(event) {
        console.log("Соединение потеряно. Повторная попытка через 10 секунд...");
        sendBtn.disabled = true;
        messageInput.disabled = true;
        
        startReconnectCountdown(10);
    };

    sendBtn.addEventListener('click', function() {
        if (socket.readyState === WebSocket.OPEN) {
            const message = messageInput.value;
            socket.send(message);
            messageInput.value = '';
        }
    });
    
}

function startReconnectCountdown(seconds) {
    countdown = seconds;
    connectionStatus.textContent = `Повторная попытка через ${countdown} секунд...`;

    reconnectInterval = setInterval(() => {
        countdown--;
        connectionStatus.textContent = `Повторная попытка через ${countdown} секунд...`;

        // Если таймер завершился, пытаемся переподключиться
        if (countdown <= 0) {
            connectionStatus.textContent = `Попытка подключения...`;
            
            clearInterval(reconnectInterval);
            tryConnect();
        }
    }, 1000);
}

// Начало попытки подключения
tryConnect();
