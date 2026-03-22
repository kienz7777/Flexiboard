import { useState, useEffect, useCallback } from 'react'

const STORAGE_KEY = 'flexiboard-layout'

export const useGridLayout = (defaultLayout) => {
  const [layout, setLayout] = useState(defaultLayout)
  const [mounted, setMounted] = useState(false)

  // Load layout from localStorage on mount
  useEffect(() => {
    const savedLayout = localStorage.getItem(STORAGE_KEY)
    if (savedLayout) {
      try {
        setLayout(JSON.parse(savedLayout))
      } catch (e) {
        console.error('Error loading saved layout:', e)
      }
    }
    setMounted(true)
  }, [])

  // Save layout to localStorage when it changes
  const handleLayoutChange = useCallback((newLayout) => {
    setLayout(newLayout)
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(newLayout))
    } catch (e) {
      console.error('Error saving layout:', e)
    }
  }, [])

  // Reset to default layout
  const resetLayout = useCallback(() => {
    setLayout(defaultLayout)
    localStorage.removeItem(STORAGE_KEY)
  }, [defaultLayout])

  return {
    layout,
    onLayoutChange: handleLayoutChange,
    resetLayout,
    mounted,
  }
}
