export interface AiChatMessage {
  role: 'user' | 'assistant'
  content: string
}

interface SseDelta {
  choices?: Array<{ delta?: { content?: string } }>
}

/**
 * Streams an AI chat completion from the backend (SSE passthrough).
 * Uses native fetch instead of the axios instance because the 15s timeout
 * and the ApiResponse envelope interceptor are incompatible with streaming.
 */
export async function streamAiChat(
  messages: AiChatMessage[],
  onDelta: (text: string) => void,
  signal: AbortSignal
): Promise<void> {
  const token = localStorage.getItem('accessToken') || sessionStorage.getItem('accessToken')

  const res = await fetch('/api/ai/chat', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: JSON.stringify({ messages }),
    signal
  })

  if (!res.ok || !res.body) {
    let message = 'AI 服务暂时不可用'
    try {
      const err = await res.json()
      if (err?.message) message = String(err.message)
    } catch {
      // non-JSON error body, keep the default message
    }
    throw new Error(message)
  }

  const reader = res.body.getReader()
  const decoder = new TextDecoder('utf-8')
  let buffer = ''

  while (true) {
    const { done, value } = await reader.read()
    if (done) break
    buffer += decoder.decode(value, { stream: true })

    const lines = buffer.split('\n')
    buffer = lines.pop() ?? '' // keep the partial last line for the next read

    for (const raw of lines) {
      const line = raw.replace(/\r$/, '')
      if (!line.startsWith('data:')) continue // blank lines and ": keep-alive" comments
      const payload = line.slice(5).trim()
      if (payload === '[DONE]') return
      try {
        const delta = (JSON.parse(payload) as SseDelta)?.choices?.[0]?.delta?.content
        if (delta) onDelta(delta)
      } catch {
        // malformed chunk — ignore
      }
    }
  }
}
