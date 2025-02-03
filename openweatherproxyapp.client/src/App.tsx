import "bootstrap/dist/css/bootstrap.min.css";
import Container from "react-bootstrap/Container";
import Weather from "./components/Weather";

function App() {
  return (
    <Container fluid className="d-flex justify-content-right align-items-right min-vh-400">
      <Weather />
    </Container>
  );
}

export default App;

