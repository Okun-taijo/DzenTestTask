import { useEffect, useState } from "react";
import { createComment, getCaptcha } from "../api/commentsApi";
import DOMPurify from "dompurify";
import { startSignalR } from "../services/signalr";

export default function CommentForm({ parentId = null, onSuccess }) {
  const [captcha, setCaptcha] = useState(null);
  const [captchaCode, setCaptchaCode] = useState("");

  const [form, setForm] = useState({
    userName: "",
    email: "",
    homePage: "",
    text: ""
  });

  const [preview, setPreview] = useState("");
  const [files, setFiles] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadCaptcha();
  }, []);

const loadCaptcha = async () => {
  try {
    const data = await getCaptcha();

    if (!data?.imageBase64) {
      console.error("Captcha empty response", data);
      return;
    }

    setCaptcha({
      captchaId: data.captchaId,
      imageBase64: data.imageBase64
    });

  } catch (err) {
    console.error("Captcha load failed:", err);
  }
};

  const validate = () => {
    const nameRegex = /^[a-zA-Z0-9]+$/;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const urlRegex = /^(https?:\/\/)?([\w.-]+)+(:\d+)?(\/\S*)?$/;

    if (!nameRegex.test(form.userName)) {
      alert("Username: only latin letters and numbers");
      return false;
    }

    if (!emailRegex.test(form.email)) {
      alert("Invalid email");
      return false;
    }

    if (form.homePage && !urlRegex.test(form.homePage)) {
      alert("Invalid homepage URL");
      return false;
    }

    if (!form.text.trim()) {
      alert("Text is required");
      return false;
    }

    if (!captchaCode) {
      alert("Captcha required");
      return false;
    }

    return true;
  };

  const sanitize = (html) => {
    return DOMPurify.sanitize(html, {
      ALLOWED_TAGS: ["a", "code", "i", "strong"],
      ALLOWED_ATTR: ["href", "title"],
    });
  };

  const updatePreview = (text) => {
    setPreview(sanitize(text));
  };

  const insertTag = (tag) => {
    if (tag === "a") {
      const template = `<a href="" title=""></a>`;
      setForm((p) => ({ ...p, text: p.text + template }));
      updatePreview(form.text + template);
      return;
    }

    const template = `<${tag}></${tag}>`;
    setForm((p) => ({ ...p, text: p.text + template }));
    updatePreview(form.text + template);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validate()) return;

    const data = new FormData();

    data.append("UserName", form.userName);
    data.append("Email", form.email);
    if (form.homePage) {
      const normalizedHomePage = /^(https?:\/\/)/i.test(form.homePage)
        ? form.homePage
        : `https://${form.homePage}`;
      data.append("HomePage", normalizedHomePage);
    }
    data.append("Text", form.text);

    if (parentId) data.append("ParentId", parentId);

    files.forEach(f => data.append("Files", f));

    data.append("CaptchaId", captcha?.captchaId || "");
    data.append("CaptchaCode", captchaCode);

    try {
      setLoading(true);
      await startSignalR();
      await createComment(data);

      setForm({ userName: "", email: "", homePage: "", text: "" });
      setFiles([]);
      setCaptchaCode("");
      setPreview("");

      await loadCaptcha();
      onSuccess?.();
    } catch (err) {
      alert(err.message);
      loadCaptcha();
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="comment-form">
      <h3>{parentId ? "Reply" : "Add Comment"}</h3>

      <form onSubmit={handleSubmit}>
        <div className="row">
          <input
            placeholder="User Name"
            value={form.userName}
            onChange={(e) =>
              setForm({ ...form, userName: e.target.value })
            }
          />

          <input
            placeholder="Email"
            value={form.email}
            onChange={(e) =>
              setForm({ ...form, email: e.target.value })
            }
          />
        </div>

        <input
          placeholder="Home page (optional)"
          value={form.homePage}
          onChange={(e) =>
            setForm({ ...form, homePage: e.target.value })
          }
        />

        <div className="toolbar">
          <button type="button" onClick={() => insertTag("i")}>i</button>
          <button type="button" onClick={() => insertTag("strong")}>b</button>
          <button type="button" onClick={() => insertTag("code")}>code</button>
          <button type="button" onClick={() => insertTag("a")}>link</button>
        </div>

        <textarea
          placeholder="Text..."
          value={form.text}
          onChange={(e) => {
            setForm({ ...form, text: e.target.value });
            updatePreview(e.target.value);
          }}
        />

        {/* PREVIEW */}
        <div className="preview">
          <span>Preview:</span>
          <div
            className="preview-box"
            dangerouslySetInnerHTML={{ __html: preview }}
          />
        </div>

        <input
          type="file"
          multiple
          onChange={(e) => setFiles([...e.target.files])}
        />

        {captcha && (
          <div className="captcha">
            <img
              src={`data:image/png;base64,${captcha.imageBase64}`}
              alt="captcha"
            />

            <input
              placeholder="Enter captcha"
              value={captchaCode}
              onChange={(e) => setCaptchaCode(e.target.value)}
            />

            <button type="button" onClick={loadCaptcha}>
              ↻
            </button>
          </div>
        )}

        <button disabled={loading}>
          {loading ? "Sending..." : "Send"}
        </button>
      </form>
    </div>
  );
}