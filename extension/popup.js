document.getElementById('openSettings').addEventListener('click', () => {
  alert('Settings would open here');
  window.close();
});
// Show status without testing native messaging
document.getElementById('status').textContent = 'Extension Active';
