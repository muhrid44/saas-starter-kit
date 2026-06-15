import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/login" element={<div>Login Page</div>} />
        <Route path="/dashboard" element={<div>Dashboard</div>} />
        <Route path="/users" element={<div>Users</div>} />
        <Route path="/audit" element={<div>Audit Logs</div>} />
      </Routes>
    </BrowserRouter>
  )
}

export default App