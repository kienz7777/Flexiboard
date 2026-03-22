import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './DataSources.css';

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  timeout: 10000,
});

export default function DataSources() {
  const [dataSources, setDataSources] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [selectedSource, setSelectedSource] = useState(null);
  const [loading, setLoading] = useState(false);
  const [testResult, setTestResult] = useState(null);
  const [formData, setFormData] = useState({
    connectorType: 'REST',
    connectionString: '',
    authenticationToken: '',
    refreshIntervalMs: 5000,
  });

  // Load data sources
  useEffect(() => {
    loadDataSources();
  }, []);

  const loadDataSources = async () => {
    try {
      setLoading(true);
      const response = await api.get('/datasources');
      setDataSources(response.data);
    } catch (error) {
      console.error('Failed to load data sources:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleTest = async () => {
    try {
      setLoading(true);
      const response = await api.post('/datasources/test', formData);
      setTestResult(response.data);
    } catch (error) {
      setTestResult({
        isSuccessful: false,
        errorMessage: error.response?.data?.error || error.message,
      });
    } finally {
      setLoading(false);
    }
  };

  const handleRegister = async () => {
    try {
      setLoading(true);
      const response = await api.post('/datasources', formData);
      setFormData({
        connectorType: 'REST',
        connectionString: '',
        authenticationToken: '',
        refreshIntervalMs: 5000,
      });
      setShowForm(false);
      setTestResult(null);
      await loadDataSources();
      alert(`✅ Data source registered: ${response.data}`);
    } catch (error) {
      alert(`❌ Failed: ${error.response?.data?.error || error.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Delete this data source?')) return;

    try {
      setLoading(true);
      await api.delete(`/datasources/${id}`);
      await loadDataSources();
    } catch (error) {
      alert(`❌ Failed: ${error.message}`);
    } finally {
      setLoading(false);
    }
  };

  const viewSourceData = async (id) => {
    try {
      const response = await api.get(`/datasources/${id}/data`);
      setSelectedSource({ id, data: response.data });
    } catch (error) {
      alert(`❌ Failed to fetch data: ${error.message}`);
    }
  };

  return (
    <div className="data-sources-container">
      <div className="data-sources-header">
        <h2>📊 Data Sources</h2>
        <p>Connect to multiple data sources (REST APIs, CSV files, Webhooks, Databases)</p>
      </div>

      {/* Add Data Source Button */}
      <button
        className="btn-primary"
        onClick={() => setShowForm(!showForm)}
        disabled={loading}
      >
        + Add Data Source
      </button>

      {/* Registration Form */}
      {showForm && (
        <div className="data-source-form">
          <h3>Register New Data Source</h3>

          <div className="form-group">
            <label>Connector Type</label>
            <select
              value={formData.connectorType}
              onChange={(e) =>
                setFormData({ ...formData, connectorType: e.target.value })
              }
            >
              <option value="REST">REST API</option>
              <option value="CSV">CSV File</option>
              <option value="Webhook">Webhook</option>
            </select>
          </div>

          <div className="form-group">
            <label>
              {formData.connectorType === 'REST' && 'API Endpoint URL'}
              {formData.connectorType === 'CSV' && 'CSV File URL or Path'}
              {formData.connectorType === 'Webhook' && 'Webhook Name/ID'}
            </label>
            <input
              type="text"
              placeholder={
                formData.connectorType === 'REST'
                  ? 'https://api.example.com/data'
                  : formData.connectorType === 'CSV'
                  ? '/path/to/file.csv or https://example.com/data.csv'
                  : 'my-webhook'
              }
              value={formData.connectionString}
              onChange={(e) =>
                setFormData({ ...formData, connectionString: e.target.value })
              }
            />
          </div>

          {formData.connectorType === 'REST' && (
            <div className="form-group">
              <label>Authentication Token (optional)</label>
              <input
                type="password"
                placeholder="Bearer token or API key"
                value={formData.authenticationToken}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    authenticationToken: e.target.value,
                  })
                }
              />
            </div>
          )}

          <div className="form-group">
            <label>Refresh Interval (ms)</label>
            <input
              type="number"
              min="1000"
              value={formData.refreshIntervalMs}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  refreshIntervalMs: parseInt(e.target.value),
                })
              }
            />
          </div>

          {/* Test Connection Button */}
          <button className="btn-secondary" onClick={handleTest} disabled={loading}>
            🔗 Test Connection
          </button>

          {testResult && (
            <div
              className={`test-result ${
                testResult.isSuccessful ? 'success' : 'error'
              }`}
            >
              <p>
                {testResult.isSuccessful
                  ? '✅ Connection successful!'
                  : `❌ ${testResult.errorMessage}`}
              </p>
              {testResult.isSuccessful && (
                <p>Response time: {testResult.responseTimeMs}ms</p>
              )}
            </div>
          )}

          {/* Register Button */}
          <div className="form-actions">
            <button
              className="btn-primary"
              onClick={handleRegister}
              disabled={loading || !formData.connectionString}
            >
              Register Data Source
            </button>
            <button
              className="btn-secondary"
              onClick={() => setShowForm(false)}
              disabled={loading}
            >
              Cancel
            </button>
          </div>
        </div>
      )}

      {/* Data Sources List */}
      <div className="data-sources-list">
        <h3>Registered Data Sources ({dataSources.length})</h3>

        {dataSources.length === 0 ? (
          <p className="empty-state">
            No data sources configured yet. Click "Add Data Source" to get started.
          </p>
        ) : (
          <div className="sources-grid">
            {dataSources.map((source) => (
              <div
                key={source.id}
                className={`source-card ${source.isActive ? 'active' : 'inactive'}`}
              >
                <div className="source-header">
                  <h4>{source.name}</h4>
                  <span className={`badge ${source.connectorType.toLowerCase()}`}>
                    {source.connectorType}
                  </span>
                </div>

                <div className="source-details">
                  <p>
                    <strong>Status:</strong>{' '}
                    {source.isActive ? '🟢 Active' : '🔴 Inactive'}
                  </p>
                  <p>
                    <strong>ID:</strong> {source.id.substring(0, 8)}...
                  </p>
                </div>

                <div className="source-actions">
                  <button
                    className="btn-small"
                    onClick={() => viewSourceData(source.id)}
                  >
                    📊 View Data
                  </button>
                  <button
                    className="btn-small btn-danger"
                    onClick={() => handleDelete(source.id)}
                  >
                    🗑️ Delete
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Data Preview Modal */}
      {selectedSource && (
        <div className="modal-overlay" onClick={() => setSelectedSource(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Data Preview: {selectedSource.id.substring(0, 8)}...</h3>
              <button
                className="btn-close"
                onClick={() => setSelectedSource(null)}
              >
                ✕
              </button>
            </div>
            <div className="modal-body">
              <pre>{JSON.stringify(selectedSource.data, null, 2)}</pre>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
