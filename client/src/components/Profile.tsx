import {useEffect, useState} from "react";
import {useAuth} from "./AuthContext.tsx";
import authClient from "../api/AuthClient.ts";
import Order from "../models/Order.ts";
import {Field} from "./Field.tsx";
import {useToast} from "./ToastContext.tsx";
import {apiError} from "../helpers/apiError.ts";
import {validateEmail} from "../helpers/email.ts";
import {validatePassword} from "../helpers/password.ts";
import "../styles/Profile.css"

/* "menu" is the pair of choices the Edit button opens, the other two are the
 * forms behind each choice */
type EditMode = "none" | "menu" | "email" | "password";

export const Profile = () => {

    const {logout} = useAuth();
    const {notify} = useToast();
    const [orders, setOrders] = useState<Order[]>([]);
    const [error, setError] = useState<string | null>(null);

    const [mode, setMode] = useState<EditMode>("none");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [busy, setBusy] = useState(false);

    useEffect(() => {
        authClient.get("/Order/MyOrders")
            .then(res => setOrders(res.data.data.map((raw: any) => new Order(raw))))
            .catch(e => console.error(e));
    }, []);

    const cancelOrder = async (orderId: string) => {
        setError(null);
        await authClient.delete(`/Order/Cancel/${orderId}`)
            .then(() => {
                setOrders(prev => prev.filter(o => o.id !== orderId));
                notify("Order cancelled", "info");
            })
            .catch(e => {
                const message = apiError(e, "Failed to cancel order");
                setError(message);
                notify(message, "error");
            });
    }

    /* every mode change clears the fields, so a half typed password can't
     * survive a trip through the menu into the other form */
    const openMode = (next: EditMode) => {
        setError(null);
        setEmail("");
        setPassword("");
        setNewPassword("");
        setMode(next);
    }

    const changeEmail = async () => {
        setError(null);

        const invalid = validateEmail(email);
        if (invalid) {
            setError(invalid);
            notify(invalid, "error");
            return;
        }

        if (password === "") {
            setError("Enter your current password to confirm");
            return;
        }

        if (!window.confirm(`Change your email to "${email.trim()}"? You will be logged out.`))
            return;

        setBusy(true);
        try {
            await authClient.put("/User/UpdateEmail", {
                email: email.trim(),
                password
            });
            notify("Email updated, please log in again", "info");
            await logout();
        } catch (e: any) {
            const message = apiError(e, "Failed to update email");
            setError(message);
            notify(message, "error");
        } finally {
            setBusy(false);
        }
    }

    const changePassword = async () => {
        setError(null);

        if (password === "") {
            setError("Enter your current password to confirm");
            return;
        }

        const invalid = validatePassword(newPassword);
        if (invalid) {
            setError(invalid);
            notify(invalid, "error");
            return;
        }

        if (!window.confirm("Change your password? You will be logged out."))
            return;

        setBusy(true);
        try {
            await authClient.put("/User/UpdatePassword", {
                oldPassword: password,
                newPassword
            });
            notify("Password updated, please log in again", "info");
            await logout();
        } catch (e: any) {
            const message = apiError(e, "Failed to update password");
            setError(message);
            notify(message, "error");
        } finally {
            setBusy(false);
        }
    }

    return (
        <div className="profile" data-testid="profile-page">
            <button className="button-main" data-testid="profile-logout" onClick={logout}>Logout</button>
            <button className="button-main" data-testid="profile-edit-toggle"
                    onClick={() => openMode(mode === "none" ? "menu" : "none")}>
                {mode === "none" ? "Edit" : "Cancel"}
            </button>

            {mode === "menu" &&
                <div className="edit-menu">
                    <button className="button-main" data-testid="profile-change-email" type="button"
                            onClick={() => openMode("email")}>Change Email</button>
                    <button className="button-main" data-testid="profile-change-password" type="button"
                            onClick={() => openMode("password")}>Change Password</button>
                </div>
            }

            {mode === "email" &&
                <div className="edit-form">
                    <h2>Change Email</h2>
                    <Field label="New Email:" value={email} onChange={setEmail}
                           testId="profile-new-email" placeholder="you@example.com"/>
                    <Field label="Password:" value={password} onChange={setPassword}
                           testId="profile-current-password" type="password" placeholder="Current password"/>
                    <div className="edit-form-buttons">
                        <button className="button-main" type="button"
                                onClick={() => openMode("menu")}>Back</button>
                        <button className="button-main" data-testid="profile-save" type="button"
                                disabled={busy} onClick={changeEmail}>
                            {busy ? "Saving…" : "Save Email"}
                        </button>
                    </div>
                </div>
            }

            {mode === "password" &&
                <div className="edit-form">
                    <h2>Change Password</h2>
                    <Field label="Current Password:" value={password} onChange={setPassword}
                           testId="profile-current-password" type="password" placeholder="Current password"/>
                    <Field label="New Password:" value={newPassword} onChange={setNewPassword}
                           testId="profile-new-password" type="password" placeholder="At least 8 characters"/>
                    <div className="edit-form-buttons">
                        <button className="button-main" type="button"
                                onClick={() => openMode("menu")}>Back</button>
                        <button className="button-main" data-testid="profile-save" type="button"
                                disabled={busy} onClick={changePassword}>
                            {busy ? "Saving…" : "Save Password"}
                        </button>
                    </div>
                </div>
            }

            {error && <p className="error" data-testid="profile-error">{error}</p>}

            <div className="my-orders" data-testid="profile-orders">
                <h2>My Orders</h2>
                {
                    orders.length === 0 ?
                        <p data-testid="profile-no-orders">No orders yet.</p> :
                        orders.map(order => (
                            <div className="order-summary" data-testid="order-summary" key={order.id}>
                                <p>{new Date(order.createdAt).toLocaleDateString()}</p>
                                <p>{order.items.length} item(s)</p>
                                <p>{order.total()} RSD</p>
                                {order.canCancel() &&
                                    <button className="button-main" data-testid="order-cancel" type="button"
                                            onClick={() => cancelOrder(order.id)}>Cancel</button>
                                }
                            </div>
                        ))
                }
            </div>
        </div>
    )
}
