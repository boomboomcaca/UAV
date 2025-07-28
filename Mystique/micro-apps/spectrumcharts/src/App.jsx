import { useState } from "react";
import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import "./App.css";
import routeContig from "./routes";
import "./index.css";
// import 'antd/dist/';

function App() {
  const [count, setCount] = useState(0);

  return (
    <div className="App">
      <div
        style={{
          width: "100%",
          height: "100%",
          color: "ButtonFace",
          backgroundColor: "#181c36",
        }}
      >
        <Router>
          <div style={{ backgroundColor: "white" }}>
            <ul>
              {routeContig.map((item) => (
                <li>
                  <Link to={item.path}>{item.name}</Link>
                </li>
              ))}
            </ul>
          </div>
          <div style={{ minHeight: "400px" }}>
            <Routes>
              {routeContig.map((item, index) => {
                const { path, Element } = item;
                return <Route path={path} element={<Element />} />;
              })}
            </Routes>
            {/* <SpectrumDemo /> */}
          </div>
        </Router>
      </div>
    </div>
  );
}

export default App;
