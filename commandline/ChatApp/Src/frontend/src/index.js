import 'bootstrap/dist/css/bootstrap.min.css';

document.addEventListener('DOMContentLoaded', () => {
  const userInput = document.getElementById('userInput');
  const messageInput = document.getElementById('messageInput');
  const sendButton = document.getElementById('sendButton');
  const messagesDiv = document.getElementById('messages');

  // Load transcript on startup
  fetch('/api/chat/transcript')
    .then(response => response.json())
    .then(messages => {
      messages.forEach(data => {
        const messageElement = document.createElement('div');
        messageElement.className = 'message';
        messageElement.innerHTML = `<strong>${escapeHtml(data.user)}</strong> (${new Date(data.timestamp).toLocaleTimeString()}): ${escapeHtml(data.message)}`;
        messagesDiv.appendChild(messageElement);
      });
      messagesDiv.scrollTop = messagesDiv.scrollHeight;
    });

  // Handle SSE connection
  const eventSource = new EventSource('/api/chat/stream');
  eventSource.onmessage = (event) => {
    const data = JSON.parse(event.data);
    const messageElement = document.createElement('div');
    messageElement.className = 'message';
    messageElement.innerHTML = `<strong>${escapeHtml(data.user)}</strong> (${new Date(data.timestamp).toLocaleTimeString()}): ${escapeHtml(data.message)}`;
    messagesDiv.appendChild(messageElement);
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
  };

  eventSource.onerror = () => {
    console.error('SSE error, attempting to reconnect...');
    eventSource.close();
  };

  // Handle form submission
  sendButton.addEventListener('click', async () => {
    const user = userInput.value.trim();
    const message = messageInput.value.trim();

    if (!user || !message) {
      alert('Please enter both a user name and message.');
      return;
    }

    const payload = { user, message };
    try {
      const response = await fetch('/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });

      if (response.ok) {
        messageInput.value = '';
      } else {
        alert('Failed to send message.');
      }
    } catch (error) {
      console.error('Error sending message:', error);
      alert('Error sending message.');
    }
  });

  // XSS prevention
  function escapeHtml(str) {
    return str
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }
});
