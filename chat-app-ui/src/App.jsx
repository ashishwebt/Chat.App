import { useState } from 'react';
import { ToastProvider } from './context/ToastContext';
import { useTheme } from './hooks/useTheme';
import { useConversations } from './hooks/useConversations';
import Sidebar from './components/Sidebar';
import TopBar from './components/TopBar';
import ChatWindow from './components/ChatWindow';

function Shell() {
  const { theme, toggle } = useTheme();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const {
    conversations,
    conversationsLoading,
    activeId,
    messages,
    conversationLoading,
    sending,
    openConversation,
    startNewConversation,
    sendMessage,
    renameConversation,
    deleteConversation,
  } = useConversations();

  return (
    <div className="h-screen flex flex-col bg-paper text-ink">
      <TopBar onMenuClick={() => setSidebarOpen(true)} theme={theme} onToggleTheme={toggle} />
      <div className="flex-1 flex min-h-0">
        <Sidebar
          open={sidebarOpen}
          onClose={() => setSidebarOpen(false)}
          conversations={conversations}
          conversationsLoading={conversationsLoading}
          activeId={activeId}
          onOpen={openConversation}
          onNew={() => {
            startNewConversation();
            setSidebarOpen(false);
          }}
          onRename={renameConversation}
          onDelete={deleteConversation}
        />
        <ChatWindow
          messages={messages}
          loading={conversationLoading}
          sending={sending}
          onSend={sendMessage}
        />
      </div>
    </div>
  );
}

export default function App() {
  return (
    <ToastProvider>
      <Shell />
    </ToastProvider>
  );
}
