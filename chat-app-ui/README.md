# Fieldnote — GenAI chat UI

A React + Tailwind front end for the chat API described in the spec: sidebar
conversation list, chat window with markdown + copy-to-clipboard, sticky
composer, light/dark theme, toasts, and full CRUD on conversations.

## Setup

```bash
npm install
npm run dev
```

The dev server proxies `/api/*` to `http://localhost:8000` (see
`vite.config.js`) — change the `target` to wherever your backend runs. In
production, serve the built app from the same origin as the API (or put both
behind one reverse proxy) so the relative `/api/...` calls keep resolving.

```bash
npm run build   # outputs to dist/
```

## Structure

```
src/
  api/client.js            fetch wrapper for every endpoint in the spec,
                            throws ApiError with a display-ready message
  hooks/
    useConversations.js     owns conversation list + active thread + all
                             mutations (send, rename, delete)
    useTheme.js              persisted light/dark theme
    useHealth.js             polls /api/health every 30s for the top bar dot
  context/ToastContext.jsx  success/error toasts, used by the hooks above
  components/
    Sidebar.jsx              conversation list, new button, mobile drawer
    ConversationItem.jsx     row with inline rename + delete
    ConfirmModal.jsx         accessible confirm dialog (used for delete)
    TopBar.jsx               wordmark, health dot, theme toggle, menu button
    ChatWindow.jsx           message list, auto-scroll, thinking indicator
    MessageBubble.jsx        role-based bubble, markdown render, copy button
    MessageInput.jsx         sticky composer, markdown preview, Enter to send
  App.jsx                    wires it all together
```

## Notes on behavior

- **Optimistic sends**: the user's bubble appears immediately; if the request
  fails it's rolled back and a toast explains why, so the composer text isn't
  lost mentally (re-type and retry).
- **New conversation**: `conversationId` is omitted on the first send; the
  hook adopts the `id`/`title` the server returns and slots it into the
  sidebar without a full reload.
- **Accessibility**: every icon-only button has an `aria-label`, the delete
  confirmation is a proper modal (focus-trapped enough for a single action,
  closes on Escape/backdrop click), and focus rings are visible everywhere
  rather than suppressed.
- **Reduced motion**: all animations (message fade-in, thinking dots) are
  disabled under `prefers-reduced-motion`.

## Design tokens

Colors and type live as CSS variables in `src/index.css` (light values on
`:root`, dark overrides under `.dark`) and are exposed to Tailwind via
`tailwind.config.js`, so `bg-accent`, `text-ink-soft`, etc. work directly in
class names and repaint automatically when the theme toggles.
