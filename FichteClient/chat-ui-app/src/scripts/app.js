class FichteClient {
    constructor() {
        this.baseUrl = 'http://localhost:5084/api';
        this.websocketUrl = 'ws://localhost:5084/api/wsmessages';
        this.token = localStorage.getItem('fichteToken');
        this.currentUser = null;
        this.currentChat = null;
        this.websocket = null;
        this.contacts = new Map();
        this.groups = new Map();
        this.onlineUsers = new Set();
        
        this.initializeElements();
        this.setupEventListeners();
        
        if (this.token) {
            this.loadUserData();
        } else {
            this.showAuth();
        }
    }
    
    initializeElements() {
        this.authContainer = document.getElementById('auth-container');
        this.mainContainer = document.getElementById('main-container');
        this.loginForm = document.getElementById('login-form');
        this.registerForm = document.getElementById('register-form');
        this.loginTab = document.getElementById('login-tab');
        this.registerTab = document.getElementById('register-tab');
        this.authError = document.getElementById('auth-error');
        this.currentUsername = document.getElementById('current-username');
        this.contactsList = document.getElementById('contacts-list');
        this.groupsList = document.getElementById('groups-list');
        this.onlineUsersList = document.getElementById('online-users-list');
        this.chatMessages = document.getElementById('chat-messages');
        this.messageInput = document.getElementById('message-input');
        this.sendButton = document.getElementById('send-button');
        this.chatTitle = document.getElementById('chat-title');
        this.contactsTab = document.getElementById('contacts-tab');
        this.groupsTab = document.getElementById('groups-tab');
        this.contactsSection = document.getElementById('contacts-section');
        this.groupsSection = document.getElementById('groups-section');
        this.createGroupModal = document.getElementById('create-group-modal');
        this.groupMembersModal = document.getElementById('group-members-modal');
        this.searchBar = document.getElementById('search-bar');
        this.searchInput = document.getElementById('search-input');
        this.groupMembersBtn = document.getElementById('group-members-btn');
    }
    
    setupEventListeners() {
        this.loginTab.addEventListener('click', () => this.switchTab('login'));
        this.registerTab.addEventListener('click', () => this.switchTab('register'));
        this.loginForm.addEventListener('submit', (e) => this.handleLogin(e));
        this.registerForm.addEventListener('submit', (e) => this.handleRegister(e));
        document.getElementById('logout-btn').addEventListener('click', () => this.logout());
        
        this.contactsTab.addEventListener('click', () => this.switchSidebarTab('contacts'));
        this.groupsTab.addEventListener('click', () => this.switchSidebarTab('groups'));
        
        document.getElementById('add-contact-btn').addEventListener('click', () => this.showAddContact());
        document.getElementById('create-group-btn').addEventListener('click', () => this.showCreateGroup());
        
        this.sendButton.addEventListener('click', () => this.sendMessage());
        this.messageInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.sendMessage();
        });
        
        document.getElementById('create-group-form').addEventListener('submit', (e) => this.createGroup(e));
        document.getElementById('cancel-group-btn').addEventListener('click', () => this.hideModal('create-group-modal'));
        document.getElementById('close-members-btn').addEventListener('click', () => this.hideModal('group-members-modal'));
        document.getElementById('leave-group-btn').addEventListener('click', () => this.leaveGroup());
        document.getElementById('copy-invite-btn').addEventListener('click', () => this.copyInviteCode());
        document.getElementById('join-group-btn').addEventListener('click', () => this.showJoinGroup());
        
        document.getElementById('search-messages-btn').addEventListener('click', () => this.toggleSearch());
        document.getElementById('close-search-btn').addEventListener('click', () => this.closeSearch());
        document.getElementById('search-btn').addEventListener('click', () => this.searchMessages());
        this.searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.searchMessages();
        });
        
        this.groupMembersBtn.addEventListener('click', () => this.showGroupMembers());
    }
    
    async apiCall(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                ...(this.token && { 'Authorization': `Bearer ${this.token}` })
            }
        };
        
        try {
            const response = await fetch(url, { ...defaultOptions, ...options });
            if (response.status === 401) {
                this.logout();
                return null;
            }
            return response;
        } catch (error) {
            console.error('API call failed:', error);
            return null;
        }
    }
    
    showAuth() {
        this.authContainer.classList.remove('hidden');
        this.mainContainer.classList.add('hidden');
    }
    
    showMain() {
        this.authContainer.classList.add('hidden');
        this.mainContainer.classList.remove('hidden');
    }
    
    switchTab(tab) {
        if (tab === 'login') {
            this.loginTab.classList.add('active');
            this.registerTab.classList.remove('active');
            this.loginForm.classList.remove('hidden');
            this.registerForm.classList.add('hidden');
        } else {
            this.registerTab.classList.add('active');
            this.loginTab.classList.remove('active');
            this.registerForm.classList.remove('hidden');
            this.loginForm.classList.add('hidden');
        }
        this.authError.textContent = '';
    }
    
    switchSidebarTab(tab) {
        if (tab === 'contacts') {
            this.contactsTab.classList.add('active');
            this.groupsTab.classList.remove('active');
            this.contactsSection.classList.remove('hidden');
            this.groupsSection.classList.add('hidden');
        } else {
            this.groupsTab.classList.add('active');
            this.contactsTab.classList.remove('active');
            this.groupsSection.classList.remove('hidden');
            this.contactsSection.classList.add('hidden');
        }
    }
    
    async handleLogin(e) {
        e.preventDefault();
        const username = document.getElementById('login-username').value;
        const password = document.getElementById('login-password').value;
        
        const response = await this.apiCall('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
        
        if (response && response.ok) {
            this.token = await response.text();
            localStorage.setItem('fichteToken', this.token);
            window.location.reload();
        } else {
            this.authError.textContent = 'Invalid username or password';
        }
    }
    
    async handleRegister(e) {
        e.preventDefault();
        const username = document.getElementById('register-username').value;
        const password = document.getElementById('register-password').value;
        const email = document.getElementById('register-email').value;
        
        const endpoint = email ? '/auth/registerwithemail' : '/auth/register';
        const body = email ? 
            JSON.stringify({ username, password, email }) :
            JSON.stringify({ username, password });
        
        const response = await this.apiCall(endpoint, {
            method: 'POST',
            body
        });
        
        if (response && response.ok) {
            this.authError.textContent = 'Registration successful! Please login.';
            this.switchTab('login');
        } else {
            const errorText = await response.text();
            this.authError.textContent = errorText || 'Registration failed';
        }
    }
    
    async loadUserData() {
        const response = await this.apiCall('/users/me');
        if (response && response.ok) {
            this.currentUser = await response.json();
            this.currentUsername.textContent = this.currentUser.username;
            this.showMain();
            await this.loadContacts();
            await this.loadGroups();
            await this.loadOnlineUsers();
            this.connectWebSocket();
        } else {
            this.logout();
        }
    }
    
    saveContactsToStorage() {
        const contactsArray = Array.from(this.contacts.values());
        localStorage.setItem(`fichteContacts_${this.currentUsername}`, JSON.stringify(contactsArray.map(c => c.id)));
    }

    async loadContacts() {
        const response = await this.apiCall('/users');
        if (response && response.ok) {
            const users = await response.json();
            const savedContactIds = JSON.parse(localStorage.getItem(`fichteContacts_${this.currentUsername}`)) ?? [];
            
            this.contacts.clear();
            users.forEach(user => {
                if (user.id !== this.currentUser.id && savedContactIds.includes(user.id)) {
                    this.contacts.set(user.id, user);
                }
            });
            this.renderContacts();
        }
    }
    
    async loadGroups() {
        const response = await this.apiCall('/group/getusergroups');
        if (response && response.ok) {
            const groups = await response.json();
            this.groups.clear();
            groups.forEach(group => {
                this.groups.set(group.id, group);
            });
            this.renderGroups();
        }
    }
    
    async loadOnlineUsers() {
        const response = await this.apiCall('/users/online');
        if (response && response.ok) {
            const users = await response.json();
            this.onlineUsers.clear();
            users.forEach(user => {
                if (user.id !== this.currentUser.id) {
                    this.onlineUsers.add(user.id);
                }
            });
            this.renderOnlineUsers();
        }
    }
    
    renderContacts() {
        this.contactsList.innerHTML = '';
        this.contacts.forEach(contact => {
            const li = document.createElement('li');
            li.className = 'contact-item';
            li.innerHTML = `
                <div class="contact-avatar"></div>
                <div class="contact-info">
                    <span class="contact-name">${contact.username}</span>
                    <span class="contact-status ${contact.isOnline ? 'online' : 'offline'}">
                        ${contact.isOnline ? 'Online' : 'Offline'}
                    </span>
                </div>
            `;
            li.addEventListener('click', () => this.openDirectChat(contact));
            this.contactsList.appendChild(li);
        });
    }
    
    renderGroups() {
        this.groupsList.innerHTML = '';
        this.groups.forEach(group => {
            const li = document.createElement('li');
            li.className = 'group-item';
            li.innerHTML = `
                <div class="group-avatar"></div>
                <div class="group-info">
                    <span class="group-name">${group.name}</span>
                    <span class="group-members">${group.memberCount} members</span>
                    <span class="group-invite-code">Code: ${group.inviteCode}</span>
                </div>
            `;
            li.addEventListener('click', () => this.openGroupChat(group));
            this.groupsList.appendChild(li);
        });
    }
    
    renderOnlineUsers() {
        this.onlineUsersList.innerHTML = '';
        this.contacts.forEach(contact => {
            if (this.onlineUsers.has(contact.id)) {
                const li = document.createElement('li');
                li.className = 'online-user';
                li.innerHTML = `
                    <div class="user-avatar"></div>
                    <span class="user-name">${contact.username}</span>
                `;
                li.addEventListener('click', () => this.openDirectChat(contact));
                this.onlineUsersList.appendChild(li);
            }
        });
    }
    
    async openDirectChat(contact) {
        this.currentChat = { type: 'direct', contact };
        this.chatTitle.textContent = contact.username;
        this.groupMembersBtn.classList.add('hidden');
        this.messageInput.disabled = false;
        this.sendButton.disabled = false;
        await this.loadMessages();
    }
    
    async openGroupChat(group) {
        this.currentChat = { type: 'group', group };
        this.chatTitle.textContent = group.name;
        this.groupMembersBtn.classList.remove('hidden');
        this.messageInput.disabled = false;
        this.sendButton.disabled = false;
        await this.loadMessages();
    }
    
    async loadMessages() {
        if (!this.currentChat) return;
        
        let endpoint = '/messages/getusermessages';
        let params = new URLSearchParams();
        
        if (this.currentChat.type === 'direct') {
            params.append('recipientUserId', this.currentChat.contact.id);
        } else {
            params.append('groupId', this.currentChat.group.id);
        }
        
        const response = await this.apiCall(`${endpoint}?${params}`);
        if (response && response.ok) {
            const messages = await response.json();
            this.renderMessages(messages);
        }
    }
    
    renderMessages(messages) {
        this.chatMessages.innerHTML = '';
        messages.forEach(message => {
            const messageDiv = document.createElement('div');
            messageDiv.className = `message ${message.userID === this.currentUser.id ? 'sent' : 'received'}`;
            
            const time = new Date(message.createdAt).toLocaleTimeString();
            const sender = message.userID === this.currentUser.id ? 'You' : 
                (message.user ? message.user.username : 'Unknown');
            
            messageDiv.innerHTML = `
                <div class="message-header">
                    <span class="message-sender">${sender}</span>
                    <span class="message-time">${time}</span>
                </div>
                <div class="message-content">${message.body}</div>
            `;
            
            this.chatMessages.appendChild(messageDiv);
        });
        
        this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
    }
    
    async sendMessage() {
        const text = this.messageInput.value.trim();
        if (!text || !this.currentChat) return;
        
        const payload = { body: text };
        if (this.currentChat.type === 'direct') {
            payload.recipientUserID = this.currentChat.contact.id;
        } else {
            payload.groupID = this.currentChat.group.id;
        }
        
        const response = await this.apiCall('/messages/sendmessage', {
            method: 'POST',
            body: JSON.stringify(payload)
        });
        
        if (response && response.ok) {
            this.messageInput.value = '';
            await this.loadMessages();
        }
    }
    
    connectWebSocket() {
        if (this.websocket) this.websocket.close();
        
        const wsUrl = `${this.websocketUrl}?token=${this.token}`;
        this.websocket = new WebSocket(wsUrl);
        
        this.websocket.onmessage = (event) => {
            const message = JSON.parse(event.data);
            this.handleWebSocketMessage(message);
        };
        
        this.websocket.onclose = () => {
            setTimeout(() => this.connectWebSocket(), 5000);
        };
    }
    
    handleWebSocketMessage(data) {
        if (data.type === 'userStatusUpdate') {
            this.handleUserStatusUpdate(data.userId, data.isOnline);
            return;
        }
        
        const message = data;
        if (this.currentChat) {
            const isRelevant = 
                (this.currentChat.type === 'direct' && 
                 (message.userID === this.currentChat.contact.id || 
                  message.recipientUserID === this.currentChat.contact.id)) ||
                (this.currentChat.type === 'group' && 
                 message.groupID === this.currentChat.group.id);
            
            if (isRelevant) {
                this.loadMessages();
            }
        }
    }
    
    handleUserStatusUpdate(userId, isOnline) {
        if (this.contacts.has(userId)) {
            const contact = this.contacts.get(userId);
            contact.isOnline = isOnline;
            this.contacts.set(userId, contact);
        }
        
        if (isOnline) {
            this.onlineUsers.add(userId);
        } else {
            this.onlineUsers.delete(userId);
        }
        
        this.renderContacts();
        this.renderOnlineUsers();
    }
    
    showAddContact() {
        const name = prompt('Enter username to add as contact:');
        if (name) {
            this.addContact(name);
        }
    }
    
    async addContact(username) {
        const response = await this.apiCall(`/users/${username}`);
        if (response && response.ok) {
            const user = await response.json();
            this.contacts.set(user.id, user);
            this.saveContactsToStorage();
            this.renderContacts();
        }
    }
    
    showCreateGroup() {
        this.createGroupModal.classList.remove('hidden');
    }
    
    async createGroup(e) {
        e.preventDefault();
        const name = document.getElementById('group-name').value;
        const description = document.getElementById('group-description').value;
        const maxMembers = parseInt(document.getElementById('group-max-members').value);
        const response = await this.apiCall('/group/creategroup', {
            method: 'POST',
            body: JSON.stringify({ name, description, maxMembers })
        });
        
        if (response && response.ok) {
            this.hideModal('create-group-modal');
            await this.loadGroups();
        }
    }
    
    async showGroupMembers() {
        if (!this.currentChat || this.currentChat.type !== 'group') return;
        
        const response = await this.apiCall(`/group/getgroupmembers?groupId=${this.currentChat.group.id}`);
        if (response && response.ok) {
            const members = await response.json();
            const membersList = document.getElementById('group-members-list');
            membersList.innerHTML = '';
            
            members.forEach(member => {
                const li = document.createElement('li');
                li.className = 'member-item';
                li.innerHTML = `
                    <div class="member-avatar"></div>
                    <span class="member-name">${member.username}</span>
                    <span class="member-status ${member.isOnline ? 'online' : 'offline'}">
                        ${member.isOnline ? 'Online' : 'Offline'}
                    </span>
                `;
                membersList.appendChild(li);
            });
            
            document.getElementById('group-invite-code').textContent = this.currentChat.group.inviteCode;
            this.groupMembersModal.classList.remove('hidden');
        }
    }
    
    async leaveGroup() {
        if (!this.currentChat || this.currentChat.type !== 'group') return;
        
        if (confirm('Are you sure you want to leave this group?')) {
            const response = await this.apiCall(`/group/leavegroup?groupId=${this.currentChat.group.id}`, {
                method: 'POST'
            });

            if (response && response.ok) {
                this.hideModal('group-members-modal');
                this.resetChat();
                await this.loadGroups();
            }
        }
    }

    resetChat(){
        this.currentChat = null;
        this.chatTitle.textContent = 'Select a conversation';
        this.chatMessages.innerHTML = '<div class="welcome-message"><h2>Welcome to Fichte Chat!</h2><p>Select a contact or group to start messaging</p></div>';
    }
    
    closeSearch() {
        this.toggleSearch();
        this.loadMessages();

    }

    toggleSearch() {
        this.searchBar.classList.toggle('hidden');
        if (!this.searchBar.classList.contains('hidden')) {
            this.searchInput.focus();
        }
    }
    
    async searchMessages() {
        const query = this.searchInput.value.trim();
        if (!query || !this.currentChat) return;
        
        let params = new URLSearchParams({ query });
        if (this.currentChat.type === 'group') {
            params.append('groupId', this.currentChat.group.id);
        }
        
        const response = await this.apiCall(`/messages/searchmessages?${params}`);
        if (response && response.ok) {
            const messages = await response.json();
            this.renderMessages(messages);
        }
    }
    
    hideModal(modalId) {
        document.getElementById(modalId).classList.add('hidden');
    }
    
    copyInviteCode() {
        if (!this.currentChat || this.currentChat.type !== 'group') return;
        
        const inviteCode = this.currentChat.group.inviteCode;
        navigator.clipboard.writeText(inviteCode).then(() => {
            alert('Invite code copied to clipboard!');
        }).catch(() => {
            const textArea = document.createElement('textarea');
            textArea.value = inviteCode;
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
            alert('Invite code copied to clipboard!');
        });
    }
    
    showJoinGroup() {
        const inviteCode = prompt('Enter group invite code:');
        if (inviteCode) {
            this.joinGroupByInvite(inviteCode);
        }
    }
    
    async joinGroupByInvite(inviteCode) {
        const response = await this.apiCall(`/group/joingroup?inviteCode=${inviteCode}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...(this.token && { 'Authorization': `Bearer ${this.token}` })
            }
        });
        
        if (response && response.ok) {
            alert('Successfully joined the group!');
            await this.loadGroups();
        } else {
            alert('Failed to join group. Please check the invite code.');
        }
    }
    
    logout() {
        if (this.websocket) this.websocket.close();
        localStorage.removeItem('fichteToken');
        this.token = null;
        this.currentUser = null;
        this.currentChat = null;
        this.showAuth();
    }
}

const app = new FichteClient();