// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const userNameInput = document.getElementById('userNameInput');
const userNameBtn = document.getElementById('userNameBtn');

const messageLog = document.getElementById('messageLog');
const messageInput = document.getElementById('messageInput');
const sendMessageBtn = document.getElementById('sendMessageBtn');
const connectionStatus = document.getElementById('connectionStatus')

let reconnectInterval; 
let countdown;
let socket = new WebSocket("ws://localhost:5232/ws");
console.log("Соединение установлено.");

userNameInput.disabled = false;
userNameBtn.disabled = false;
sendMessageBtn.disabled = true;
messageInput.disabled = true;

userNameBtn.addEventListener('click', function(){
    if (userNameInput.value !== ''){
        const joinChatRequest = {
            Command: "joinChat",
            Name: userNameInput.value
        }
        socket.send(JSON.stringify(joinChatRequest));
    }
})

socket.onopen = function() {
    console.log("Соединение установлено.");
    connectionStatus.textContent = "Соединение установлено."; // Обновляем статус
    clearInterval(reconnectInterval);
    connectionStatus.textContent = "";
};

socket.onmessage = function(event) {
    let data = JSON.parse(event.data);

    if (data.Command === "history") {
        data.ChatHistory.forEach(msg => {
            let message = `${msg.Source}: ${msg.Text}`;
            messageLog.value += message + '\n';
        });
        
        userNameInput.disabled = true;
        userNameBtn.disabled = true;
        sendMessageBtn.disabled = false;
        messageInput.disabled = false;
    }

    if (data.Command === "receiveMessage") {
        let message;

        if (data.Target !== null) {
            message = `@Private From ${data.Source} to ${data.Target}: ${data.Text}`;
        } else {
            message = `${data.Source}: ${data.Text}`;
        }

        messageLog.value += message + '\n'
    }

    if (data.Command === "error") {
        connectionStatus.textContent = `Ошибка в ${data.SourceCommand}: ${data.ErrorText}`;

        if (data.SourceCommand === "joinChat"){
            userNameInput.disabled = false;
            userNameBtn.disabled = false;
            sendMessageBtn.disabled = true;
            messageInput.disabled = true;
        }
    }

};

socket.onclose = function(event) {
    console.log("Соединение потеряно. Повторная попытка через 10 секунд...");
    userNameInput.disabled = true;
    userNameBtn.disabled = true;
    sendMessageBtn.disabled = true;
    messageInput.disabled = true;

    startReconnectCountdown(10);
};

sendMessageBtn.addEventListener('click', function() {
    if (socket.readyState === WebSocket.OPEN) {
        const message = messageInput.value;

        if (message.startsWith('@')){
            const parts = message.split(' ');
            const target = parts[0].substring(1);
            const text = parts.slice(1).join(' ');

            const chatMessage = {
                Source: userNameInput.value,
                Target: target,
                Text: text
            }

            const privateMessageRequest = {
                Command: "sendPrivateMessage",
                ChatMessage: chatMessage
            }

            socket.send(JSON.stringify(privateMessageRequest));
        }

        else {
            const chatMessage = {
                Source: userNameInput.value,
                Target: null,
                Text: message
            }

            const publicMessageRequest = {
                Command: "sendPublicMessage",
                ChatMessage: chatMessage
            }

            socket.send(JSON.stringify(publicMessageRequest));
        }
        messageInput.value = '';
    }
});
    


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
            socket = new WebSocket("ws://localhost:5232/ws");
        }
    }, 1000);
}


