import { useState, useEffect, useCallback, useRef } from 'react'
import * as signalR from '@microsoft/signalr'

export const useRealtime = (url = 'http://localhost:5000/hub/dashboard') => {
  const [data, setData] = useState({
    totalOrders: 0,
    totalRevenue: 0,
    totalUsers: 0,
    revenueTrends: [],
    lastUpdated: new Date().toISOString()
  })
  const [connected, setConnected] = useState(false)
  const [error, setError] = useState(null)
  const hubRef = useRef(null)

  // Fetch initial dashboard data via HTTP
  const fetchInitialData = useCallback(async () => {
    try {
      const response = await fetch('http://localhost:5000/api/dashboard')
      if (response.ok) {
        const initialData = await response.json()
        console.log('Fetched initial dashboard data:', initialData)
        if (initialData) {
          setData(initialData)
        }
      } else {
        console.error('Dashboard API returned status:', response.status)
      }
    } catch (err) {
      console.error('Error fetching initial dashboard data:', err)
    }
  }, [])

  useEffect(() => {
    // Fetch initial data immediately
    fetchInitialData()

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        withCredentials: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build()

    hubRef.current = connection

    connection.on('ReceiveDashboardUpdate', (dashboardData) => {
      console.log('Received dashboard update:', dashboardData)
      setData(dashboardData)
      setError(null)
    })

    connection.onreconnecting((error) => {
      console.log('Reconnecting to SignalR...', error)
      setConnected(false)
    })

    connection.onreconnected(() => {
      console.log('Reconnected to SignalR')
      setConnected(true)
      setError(null)
    })

    connection.onclose((error) => {
      console.error('Connection closed', error)
      setConnected(false)
      if (error) {
        setError(error.toString())
      }
    })

    connection.start()
      .then(() => {
        console.log('Connected to SignalR hub')
        setConnected(true)
        setError(null)
      })
      .catch(err => {
        console.error('Error connecting to SignalR:', err)
        setError(`Failed to connect: ${err.message}`)
        setConnected(false)
      })

    return () => {
      if (connection) {
        connection.stop()
      }
    }
  }, [url, fetchInitialData])

  return { data, connected, error }
}
