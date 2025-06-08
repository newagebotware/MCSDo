import 'bootstrap/dist/css/bootstrap.min.css';

document.addEventListener('DOMContentLoaded', () => {
  const userInput = document.getElementById('userInput');
  const messageInput = document.getElementById('messageInput');
  const sendButton = document.getElementById('sendButton');
  const messagesDiv = document.getElementById('messages');

  // Handle SSE connection
  const eventSource = new EventSource('/api/chat/stream');

  eventSource.onmessage = (event) => {
    const data = JSON.parse(event.data);
    const messageElement = document.createElement('div');
    messageElement.className = 'message'; // Use custom class for styling
    messageElement.innerHTML = `<strong>${data.user}</strong> (${new Date(data.timestamp).toLocaleTimeString()}): ${data.message}`;
    messagesDiv.appendChild(messageElement);
    messagesDiv.scrollTop = messagesDiv.scrollHeight; // Auto-scroll to bottom
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
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (response.ok) {
        messageInput.value = ''; // Clear message input
      } else {
        alert('Failed to send message.');
      }
    } catch (error) {
      console.error('Error sending message:', error);
      alert('Error sending message.');
    }
  });
});
