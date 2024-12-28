import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import Busqueda from './pages/Busqueda';
import Carga from './pages/Carga';
import './style.css'

function App() {
  return (
    <Router>
      <div>
        <Routes>
          <Route path="/" element={<Carga />} />
          <Route path="/busqueda" element={<Busqueda />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
