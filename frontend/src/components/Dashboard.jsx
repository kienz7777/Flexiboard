import React, { useState, useEffect } from 'react'
import MetricCard from '../widgets/MetricCard'
import RevenueChart from '../widgets/RevenueChart'
import { useRealtime } from '../hooks/useRealtime'
import './Dashboard.css'

const Dashboard = () => {
  const { data, connected, error } = useRealtime()

  const calculateMetrics = () => {
    if (!data || data.totalOrders === 0) {
      return {
        avgOrderValue: 0,
        conversionRate: 0,
        activeUsers: 0
      }
    }
    
    const avgOrderValue = data.totalOrders > 0 ? data.totalRevenue / data.totalOrders : 0
    const conversionRate = data.totalUsers > 0 ? (data.totalOrders / (data.totalUsers * 5)) * 100 : 0
    const activeUsers = Math.floor(data.totalUsers * 0.6)

    return {
      avgOrderValue: isNaN(avgOrderValue) ? 0 : avgOrderValue,
      conversionRate: isNaN(conversionRate) ? 0 : Math.min(conversionRate, 100),
      activeUsers: isNaN(activeUsers) ? 0 : activeUsers
    }
  }

  const calculateTrend = (trendData, valueKey, lookbackDays = 30) => {
    if (!trendData || trendData.length < 2) return null

    const sorted = [...trendData].sort((a, b) => new Date(a.date) - new Date(b.date))
    const lastDate = new Date(sorted[sorted.length - 1].date)

    const currentStart = new Date(lastDate)
    currentStart.setDate(currentStart.getDate() - lookbackDays + 1)

    const previousEnd = new Date(currentStart)
    previousEnd.setDate(previousEnd.getDate() - 1)

    const previousStart = new Date(previousEnd)
    previousStart.setDate(previousStart.getDate() - lookbackDays + 1)

    const sumRange = (start, end, key) =>
      sorted
        .filter(item => {
          const d = new Date(item.date)
          return d >= start && d <= end
        })
        .reduce((acc, item) => acc + Number(item[key] || 0), 0)

    const currentValue = sumRange(currentStart, lastDate, valueKey)
    const previousValue = sumRange(previousStart, previousEnd, valueKey)

    if (currentValue === 0 && previousValue === 0) return null

    let percentChange
    let direction

    if (previousValue === 0) {
      percentChange = 100
      direction = currentValue >= 0 ? 'up' : 'down'
    } else {
      const change = ((currentValue - previousValue) / previousValue) * 100
      percentChange = Math.abs(change)
      direction = change >= 0 ? 'up' : 'down'
    }

    return {
      direction,
      percent: Math.round(percentChange * 10) / 10,
      label: 'from last month'
    }
  }

  const revenueTrend = calculateTrend(data?.revenueTrends || [], 'revenue', 30)
  const ordersTrend = calculateTrend(data?.revenueTrends || [], 'orderCount', 30)
  const metrics = calculateMetrics()

  return (
    <div className="dashboard-container">
      <div className="dashboard-branding">
        <h1 className="dashboard-title">📊 FlexiBoard Pro</h1>
      </div>
      <div className="dashboard-header">
        <div className="header-left">
          <span className={`connection-status ${connected ? 'connected' : 'disconnected'}`}>
            {connected ? '● Live' : '● Offline'}
          </span>
          <button className="btn btn-secondary" title="Settings">
            ⚙️ Settings
          </button>
          <button className="btn btn-primary">
            + Add Widget
          </button>
        </div>
        <div className="header-right">
          <span className="welcome-text">Welcome, Admin</span>
          <div className="user-avatar">A</div>
        </div>
      </div>

      {error && (
        <div className="error-banner">
          ⚠️ Connection error: {error}
        </div>
      )}

      <div className="dashboard-content">
        <div className="metrics-grid">
          <MetricCard
            title="Total Orders"
            value={data?.totalOrders || 0}
            icon="📦"
            type="number"
            trend={ordersTrend}
          />

          <MetricCard
            title="Total Revenue"
            value={data?.totalRevenue || 0}
            icon="💰"
            type="currency"
            trend={revenueTrend}
          />

          <MetricCard
            title="Total Users"
            value={data?.totalUsers || 0}
            icon="👥"
            type="number"
          />

          <MetricCard
            title="Avg Order Value"
            value={metrics.avgOrderValue || 0}
            icon="💵"
            type="currency"
          />

          <MetricCard
            title="Active Users"
            value={metrics.activeUsers || 0}
            icon="⚡"
            type="number"
          />

          <MetricCard
            title="Conversion Rate"
            value={metrics.conversionRate || 0}
            icon="📈"
            type="percentage"
          />
        </div>

        <div className="chart-container">
          <RevenueChart data={data?.revenueTrends || []} />
        </div>
        <div className="footer-center">
          <span className="last-updated">
            {data?.lastUpdated ? `Last updated: ${new Date(data.lastUpdated).toLocaleTimeString()}` : 'Waiting for data...'}
          </span>
        </div>
        <div className="footer-right">
          <span className="realtime-indicator">⚡ Real-time sync enabled (SignalR)</span>
        </div>
      </div>
    </div>
  )
}

export default Dashboard
