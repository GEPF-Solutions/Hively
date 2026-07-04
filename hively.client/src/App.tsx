import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import { SignalRProvider } from './contexts/SignalRContext';
import { ProtectedRoute, ScrollToTop } from './components/navigation';
import { ToastContainer } from './components/ui';
import Login from './pages/Login';
import Topics from './pages/Topics';
import Graph from './pages/Graph';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <SignalRProvider>
          <ToastProvider>
            <ScrollToTop />
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route
                path="/topics/*"
                element={
                  <ProtectedRoute>
                    <Topics />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/graph"
                element={
                  <ProtectedRoute>
                    <Graph />
                  </ProtectedRoute>
                }
              />
              <Route path="/" element={<Navigate to="/topics" replace />} />
              <Route path="*" element={<Navigate to="/topics" replace />} />
            </Routes>
            <ToastContainer />
          </ToastProvider>
        </SignalRProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}
