// Listen for download events
chrome.downloads.onCreated.addListener((downloadItem) => {
  console.log('Download detected:', downloadItem);
  // Cancel the browser download
  chrome.downloads.cancel(downloadItem.id, () => {
    console.log('Download cancelled:', downloadItem.id);
  });
  // Send to native host
  var port = chrome.runtime.connectNative('com.hits.downloadmanager');
  port.postMessage({
    action: 'download',
    url: downloadItem.url,
    filename: downloadItem.filename || 'download',
    referrer: downloadItem.referrer || ''
  });
  port.onMessage.addListener((response) => {
    console.log('Native host response:', response);
    chrome.notifications.create({
      type: 'basic',
      iconUrl: 'icons/icon48.png',
      title: 'Hits Download Manager',
      message: 'Download started: ' + (downloadItem.filename || 'file')
    });
  });
  port.onDisconnect.addListener(() => {
    console.log('Native host disconnected');
    if (chrome.runtime.lastError) {
      console.error('Connection error:', chrome.runtime.lastError.message);
    }
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
      iconUrl: 'icons/icon48.png',
      title: 'Hits Download Manager',
      message: 'Download started'
    });
  }
});
console.log('Hits Download Manager background script loaded');
