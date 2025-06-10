const contacts = [
    { name: "Darshan Zalavadiya", messages: [
        { sender: "user", text: "Hello, Darshan" },
        { sender: "bot", text: "Hello" },
        { sender: "user", text: "How are you" },
        { sender: "bot", text: "I am good" },
        { sender: "bot", text: "What about You" },
        { sender: "user", text: "Same for this side" },
        { sender: "bot", text: "Good" }
    ]},
    { name: "School App Client", messages: [
        { sender: "user", text: "Hi, School App Client" },
        { sender: "bot", text: "Hello! How can I help you?" }
    ]},
    { name: "UI/UX Teams", messages: [
        { sender: "user", text: "Hey team, any updates?" },
        { sender: "bot", text: "I have done my work 👍" }
    ]}
];

let currentContactIndex = 0;

const contactsList = document.getElementById('contacts-list');
const chatMessages = document.getElementById('chat-messages');
const messageInput = document.getElementById('message-input');
const sendButton = document.getElementById('send-button');
const chatTitle = document.getElementById('chat-title');
const addContactBtn = document.getElementById('add-contact-btn');
const addContactForm = document.getElementById('add-contact-form');
const newContactName = document.getElementById('new-contact-name');

function renderContacts() {
    contactsList.innerHTML = '';
    contacts.forEach((contact, idx) => {
        const li = document.createElement('li');
        li.textContent = contact.name;
        if (idx === currentContactIndex) li.classList.add('active');
        li.addEventListener('click', () => {
            currentContactIndex = idx;
            renderContacts();
            renderMessages();
        });
        contactsList.appendChild(li);
    });
}

function renderMessages() {
    chatMessages.innerHTML = '';
    chatTitle.textContent = contacts[currentContactIndex].name;
    contacts[currentContactIndex].messages.forEach(msg => {
        const msgDiv = document.createElement('div');
        msgDiv.className = `chat-message ${msg.sender}`;
        const innerDiv = document.createElement('div');
        innerDiv.className = 'message';
        innerDiv.textContent = msg.text;
        msgDiv.appendChild(innerDiv);
        chatMessages.appendChild(msgDiv);
    });
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function sendMessage() {
    const text = messageInput.value.trim();
    if (text) {
        contacts[currentContactIndex].messages.push({ sender: "user", text });
        renderMessages();
        messageInput.value = '';
        setTimeout(() => {
            contacts[currentContactIndex].messages.push({ sender: "bot", text: "This is a response to: " + text });
            renderMessages();
        }, 700);
    }
}

sendButton.addEventListener('click', sendMessage);
messageInput.addEventListener('keydown', function(e) {
    if (e.key === 'Enter') sendMessage();
});

// Add Contact Feature
addContactBtn.addEventListener('click', () => {
    addContactForm.style.display = addContactForm.style.display === 'none' ? 'block' : 'none';
    newContactName.value = '';
    newContactName.focus();
});

addContactForm.addEventListener('submit', function(e) {
    e.preventDefault();
    const name = newContactName.value.trim();
    if (name) {
        contacts.push({ name, messages: [] });
        currentContactIndex = contacts.length - 1;
        renderContacts();
        renderMessages();
        addContactForm.style.display = 'none';
    }
});

// Initial render
renderContacts();
renderMessages();