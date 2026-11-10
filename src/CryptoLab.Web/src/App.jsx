import { useEffect, useState } from "react";
import "./App.css";

const API = (window.APP_CONFIG && window.APP_CONFIG.apiUrl) || "/api";

export default function App() {
  const [keys, setKeys] = useState([]);
  const [name, setName] = useState("");
  const [editId, setEditId] = useState(null);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  const request = async (path, options) => {
    const r = await fetch(`${API}${path}`, options);
    if (!r.ok) throw new Error(`HTTP ${r.status}`);
    return r.status === 204 ? null : r.json();
  };

  const load = async () => {
    try {
      setKeys(await request("/keys"));
      setError("");
    } catch (e) {
      setError("Не вдалося завантажити ключі: " + e.message);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const save = async (e) => {
    e.preventDefault();
    setBusy(true);
    try {
      await request(editId === null ? "/keys" : `/keys/${editId}`, {
        method: editId === null ? "POST" : "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name }),
      });
      setName("");
      setEditId(null);
      await load();
    } catch (e) {
      setError("Помилка збереження: " + e.message);
    } finally {
      setBusy(false);
    }
  };

  const edit = (k) => {
    setEditId(k.id);
    setName(k.name);
  };

  const cancel = () => {
    setEditId(null);
    setName("");
  };

  const remove = async (id) => {
    try {
      await request(`/keys/${id}`, { method: "DELETE" });
      await load();
    } catch (e) {
      setError("Помилка видалення: " + e.message);
    }
  };

  return (
    <div className="page">
      <h1>CryptoLab: RSA-ключі</h1>
      <p className="hint">API: {API}</p>
      {error && <p className="error">{error}</p>}

      <form onSubmit={save} className="form">
        <input
          placeholder="Назва ключа"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
        <button type="submit" disabled={busy}>
          {editId === null ? "Згенерувати ключ" : "Зберегти назву"}
        </button>
        {editId !== null && (
          <button type="button" onClick={cancel}>
            Скасувати
          </button>
        )}
      </form>

      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Назва</th>
            <th>Публічний ключ</th>
            <th>Дії</th>
          </tr>
        </thead>
        <tbody>
          {keys.map((k) => (
            <tr key={k.id}>
              <td>{k.id}</td>
              <td>{k.name}</td>
              <td>
                <code>{k.publicKey.slice(0, 40)}…</code>
              </td>
              <td>
                <button onClick={() => edit(k)}>Редагувати</button>{" "}
                <button onClick={() => remove(k.id)}>Видалити</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {keys.length === 0 && !error && <p className="hint">Ключів немає.</p>}
    </div>
  );
}