(function(){
    function formatTs(iso){ try{ const d = new Date(iso); return d.toLocaleString(); } catch { return iso; } }
    function el(tag, cls, html){ const e = document.createElement(tag); if(cls) e.className = cls; if(html) e.innerHTML = html; return e; }
    function renderMessage(conv, msg, who){
        const wrapper = el('div', 'ai-msg ' + (who==='user' ? 'user' : 'bot'));
        if(who==='bot') wrapper.innerHTML = marked.parse(msg.text || ''); else wrapper.textContent = msg.text || '';
        const ts = el('span','ts', formatTs(msg.createdAt)); wrapper.appendChild(ts);
        conv.appendChild(wrapper); conv.scrollTop = conv.scrollHeight;
    }
    function showLoading(conv){ const d = el('div','ai-msg bot'); d.innerHTML = '<span class="ai-loading-dots"><span></span><span></span><span></span></span>'; conv.appendChild(d); conv.scrollTop = conv.scrollHeight; return d; }

    function showAuthRequired(conv){
        conv.innerHTML = '';
        const box = el('div','ai-empty','Bạn chưa đăng nhập. <button class="btn btn-sm btn-primary ms-2" id="ai-login-btn">Đăng nhập</button>');
        conv.appendChild(box);
        const btn = box.querySelector('#ai-login-btn');
        if(btn) btn.addEventListener('click', function(){ window.location.href = '/Account/Login'; });
    }

    function showInlineError(conv, message){
        // remove any previous error
        const prev = conv.querySelector('.ai-error-banner');
        if(prev) prev.remove();
        const b = el('div','ai-error-banner', message);
        b.style.margin = '12px auto';
        conv.insertBefore(b, conv.firstChild);
        // auto-remove after 8s
        setTimeout(()=>{ b.remove(); }, 8000);
    }

    async function loadHistory(conv){
        conv.innerHTML = '';
        try{
            const res = await fetch('/ai/history');
            if(res.status === 401){ showAuthRequired(conv); return; }
            if(!res.ok){ conv.appendChild(el('div','ai-empty','Không thể tải lịch sử.')); return; }
            const items = await res.json();
            if(!items || items.length===0){ conv.appendChild(el('div','ai-empty','Bắt đầu một cuộc trò chuyện mới với trợ lý AI.')); return; }
            items.forEach(it => { if(it.UserMessage) renderMessage(conv, { text: it.UserMessage, createdAt: it.CreatedAt }, 'user'); if(it.AiReply) renderMessage(conv, { text: it.AiReply, createdAt: it.CreatedAt }, 'bot'); });
        }catch(e){ conv.appendChild(el('div','ai-empty','Lỗi tải lịch sử.')); }
    }

    function init(root){
        const openBtn = root.querySelector('#ai-open-btn');
        const panel = root.querySelector('#ai-panel');
        const closeBtn = root.querySelector('#ai-close');
        const conv = root.querySelector('#ai-conversation');
        const input = root.querySelector('#ai-input');
        const sendBtn = root.querySelector('#ai-send');
        const exportBtn = root.querySelector('#ai-export');

        if(openBtn){
            openBtn.addEventListener('click', async function(){ panel.style.display='flex'; openBtn.style.display='none'; await loadHistory(conv); input.focus(); });
        }
        if(closeBtn && openBtn){
            closeBtn.addEventListener('click', function(){ panel.style.display='none'; openBtn.style.display='block'; });
        }

        // Page mode: if root is marked as page or there is no open button, open panel and load history immediately
        const isPageMode = root.hasAttribute('data-ai-chat-page') || root.classList.contains('ai-fullpage-root') || !openBtn;
        if(isPageMode){
            if(panel) panel.style.display = 'flex';
            if(openBtn) openBtn.style.display = 'none';
            if(conv) loadHistory(conv);
            if(input) input.focus();
        }

        sendBtn.addEventListener('click', sendMessage);
        input.addEventListener('keydown', function(e){ if(e.key==='Enter' && !e.shiftKey){ e.preventDefault(); sendMessage(); } });

        let pending = false;
        async function sendMessage(){
            if(pending) return; const txt = (input.value||'').trim(); if(!txt) return; renderMessage(conv, { text: txt, createdAt: new Date().toISOString() }, 'user'); input.value = ''; pending = true; const loader = showLoading(conv);
            try{ const res = await fetch('/ai/chat', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ message: txt }) });
                    if(res.status === 401){ loader.remove(); showAuthRequired(conv); return; }
                    if(res.status === 502){ loader.remove(); showInlineError(conv, 'Có lỗi khi liên hệ trợ lý AI. Vui lòng thử lại sau.'); return; }
                    if(!res.ok){ const err = await res.text(); throw new Error(err || 'Lỗi'); }
                    const data = await res.json(); loader.remove(); renderMessage(conv, { text: data.reply || 'Không có phản hồi.', createdAt: new Date().toISOString() }, 'bot'); }
            catch(err){ loader.remove(); renderMessage(conv, { text: 'Lỗi: ' + (err.message||err), createdAt: new Date().toISOString() }, 'bot'); }
            finally{ pending = false; }
        }

        exportBtn.addEventListener('click', function(){ let text=''; const nodes = conv.querySelectorAll('.ai-msg'); nodes.forEach(n =>{ const who = n.classList.contains('user') ? 'You' : 'AI'; text += who + ': ' + n.textContent.replace(/\n+/g,' ') + '\n'; }); const blob = new Blob([text], { type:'text/plain' }); const url = URL.createObjectURL(blob); const a = document.createElement('a'); a.href = url; a.download = 'chat_' + new Date().toISOString().slice(0,10) + '.txt'; a.click(); URL.revokeObjectURL(url); });

        conv.addEventListener('click', function(e){ const m = e.target.closest('.ai-msg.bot'); if(m){ navigator.clipboard.writeText(m.textContent || ''); } });
    }

    document.addEventListener('DOMContentLoaded', function(){
        const roots = document.querySelectorAll('[data-ai-chat-root]');
        roots.forEach(r => init(r));
    });
})();
