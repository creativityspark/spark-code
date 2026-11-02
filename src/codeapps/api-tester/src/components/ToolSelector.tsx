import { useEffect, useId, useRef, useState } from 'react'
import type { KeyboardEvent } from 'react'
import { tools } from '../tools/registry'
import type { ToolId } from '../tools/registry'

interface ToolSelectorProps {
  activeToolId: ToolId
  onSelectTool: (toolId: ToolId) => void
}

export function ToolSelector({ activeToolId, onSelectTool }: ToolSelectorProps) {
  const [isOpen, setIsOpen] = useState(false)
  const rootRef = useRef<HTMLDivElement>(null)
  const triggerRef = useRef<HTMLButtonElement>(null)
  const itemRefs = useRef<(HTMLButtonElement | null)[]>([])
  const openingIndex = useRef(0)
  const menuId = useId()
  const triggerId = useId()

  useEffect(() => {
    if (!isOpen) return

    itemRefs.current[openingIndex.current]?.focus()
    const dismissOutside = (event: PointerEvent) => {
      if (event.target instanceof Node && !rootRef.current?.contains(event.target)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('pointerdown', dismissOutside)
    return () => document.removeEventListener('pointerdown', dismissOutside)
  }, [isOpen])

  function openMenu(index: number) {
    openingIndex.current = index
    setIsOpen(true)
  }

  function closeAndRestoreFocus() {
    setIsOpen(false)
    triggerRef.current?.focus()
  }

  function handleMenuKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    const index = itemRefs.current.findIndex((item) => item === document.activeElement)
    let nextIndex: number
    switch (event.key) {
      case 'ArrowDown': nextIndex = (index + 1) % tools.length; break
      case 'ArrowUp': nextIndex = (index - 1 + tools.length) % tools.length; break
      case 'Home': nextIndex = 0; break
      case 'End': nextIndex = tools.length - 1; break
      case 'Escape':
        event.preventDefault()
        event.stopPropagation()
        closeAndRestoreFocus()
        return
      default: return
    }
    event.preventDefault()
    itemRefs.current[nextIndex]?.focus()
  }

  return (
    <div
      className="tool-selector"
      ref={rootRef}
      onBlur={(event) => {
        if (!event.currentTarget.contains(event.relatedTarget)) setIsOpen(false)
      }}
    >
      <button
        type="button"
        className="button"
        id={triggerId}
        ref={triggerRef}
        aria-haspopup="menu"
        aria-expanded={isOpen}
        aria-controls={menuId}
        onClick={() => {
          if (isOpen) closeAndRestoreFocus()
          else openMenu(tools.findIndex((tool) => tool.id === activeToolId))
        }}
        onKeyDown={(event) => {
          if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault()
            openMenu(event.key === 'ArrowDown' ? 0 : tools.length - 1)
          } else if (event.key === 'Escape' && isOpen) {
            event.preventDefault()
            closeAndRestoreFocus()
          }
        }}
      >
        Tools <span aria-hidden="true" className="chevron" />
      </button>
      {isOpen && (
        <div
          className="tool-menu"
          id={menuId}
          role="menu"
          aria-labelledby={triggerId}
          onKeyDown={handleMenuKeyDown}
        >
          {tools.map((tool, index) => (
            <button
              type="button"
              key={tool.id}
              role="menuitemradio"
              aria-checked={tool.id === activeToolId}
              tabIndex={-1}
              ref={(element) => { itemRefs.current[index] = element }}
              onClick={() => {
                onSelectTool(tool.id)
                closeAndRestoreFocus()
              }}
            >
              {tool.label}
              {tool.id === activeToolId && <span aria-hidden="true">✓</span>}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
