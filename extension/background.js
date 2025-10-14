// Listen for download events
chrome.downloads.onCreated.addListener((downloadItem) => {
  console.log('Download detected:', downloadItem);
  // Cancel the browser download
  chrome.downloads.cancel(downloadItem.id, () => {
    console.log('Download cancelled:', downloadItem.id);
  });
  // Log the download info (instead of sending to native host for now)
  console.log('Download info:', {
    url: downloadItem.url,
    filename: downloadItem.filename,
    referrer: downloadItem.referrer
  });
  // Show notification
  chrome.notifications.create({
    type: 'basic',
    title: 'Hits Download Manager',
    message: `Download intercepted: ${downloadItem.filename || 'file'}`
  }, (notificationId) => {
    console.log('Notification shown:', notificationId);
  });
});
// Context menu for links
chrome.runtime.onInstalled.addListener(() => {
  chrome.contextMenus.create({
    id: 'downloadWithHits',
    title: 'Download with Hits Download Manager',
    contexts: ['link']
  });
  console.log('Context menu created');
});
chrome.contextMenus.onClicked.addListener((info, tab) => {
  if (info.menuItemId === 'downloadWithHits') {
    console.log('Context menu clicked:', info.linkUrl);
    chrome.notifications.create({
      type: 'basic',
      title: 'Hits Download Manager',
      message: 'Download would start: ' + info.linkUrl.split('/').pop()
    });
  }
});
console.log('Hits Download Manager background script loaded');
