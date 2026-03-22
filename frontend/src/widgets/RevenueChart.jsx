import React from 'react'
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  Area,
  AreaChart,
} from 'recharts'
import './RevenueChart.css'
import { formatCurrency, formatDate } from '../utils/formatters'

const RevenueChart = ({ data = [] }) => {
  const chartData = data.map(item => ({
    date: formatDate(item.date),
    revenue: item.revenue,
    orders: item.orderCount,
  })) || []

  const CustomTooltip = ({ active, payload }) => {
    if (active && payload && payload.length) {
      return (
        <div className="custom-tooltip">
          <p className="label">{payload[0].payload.date}</p>
          <p className="value">Revenue: {formatCurrency(payload[0].value)}</p>
          {payload[1] && (
            <p className="value">Orders: {payload[1].value}</p>
          )}
        </div>
      )
    }
    return null
  }

  return (
    <div className="revenue-chart">
      <h3 className="chart-title">Revenue Trend (Last 30 Days)</h3>
      <div className="chart-wrapper">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={chartData}>
            <defs>
              <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.8} />
                <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
            <XAxis
              dataKey="date"
              stroke="#999"
              style={{ fontSize: '12px' }}
            />
            <YAxis
              stroke="#999"
              style={{ fontSize: '12px' }}
              tickFormatter={(value) => `$${(value / 1000).toFixed(0)}k`}
            />
            <Tooltip content={<CustomTooltip />} />
            <Legend />
            <Area
              type="monotone"
              dataKey="revenue"
              stroke="#3b82f6"
              fillOpacity={1}
              fill="url(#colorRevenue)"
              name="Revenue"
            />
            <Line
              type="monotone"
              dataKey="orders"
              stroke="#10b981"
              yAxisId="right"
              strokeWidth={2}
              name="Orders"
              dot={{ r: 4 }}
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}

export default RevenueChart
