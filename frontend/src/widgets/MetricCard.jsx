import React from 'react'
import './MetricCard.css'
import { formatCurrency, formatNumber, formatPercentage } from '../utils/formatters'

const MetricCard = ({ title, value, icon, type = 'number', trend = null }) => {
  // Ensure value is always a number
  let numericValue = 0
  if (value !== null && value !== undefined) {
    numericValue = Number(value)
    if (isNaN(numericValue)) {
      numericValue = 0
    }
  }

  let formattedValue
  if (type === 'currency') {
    formattedValue = formatCurrency(numericValue)
  } else if (type === 'percentage') {
    formattedValue = formatPercentage(numericValue)
  } else {
    formattedValue = formatNumber(numericValue)
  }

  console.log(`MetricCard [${title}] - value: ${value}, numeric: ${numericValue}, formatted: ${formattedValue}, type: ${type}`)

  return (
    <div className="metric-card">
      <div className="metric-header">
        {icon && <span className="metric-icon">{icon}</span>}
        <h3 className="metric-title">{title}</h3>
      </div>
      <div className="metric-content">
        <span className="metric-value">{formattedValue}</span>
        {trend && (
          <p className={`metric-trend trend-${trend.direction}`}>
            <span className="trend-icon">{trend.direction === 'up' ? '↑' : '↓'}</span>
            {trend.percent}% {trend.label}
          </p>
        )}
      </div>
    </div>
  )
}

export default MetricCard
