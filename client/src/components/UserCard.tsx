import "../styles/UserCard.css"
import {useState} from "react";
import User from "../models/User.ts";
import authClient from "../api/AuthClient.ts";
import {useAuth} from "./AuthContext.tsx";
import {useToast} from "./ToastContext.tsx";
import {apiError} from "../helpers/apiError.ts";

type UserCardProps = {
    user: User;
    onUpdated: (user: User) => void;
    onDeleted: (id: string) => void;
}

export const UserCard = ({user, onUpdated, onDeleted}: UserCardProps) => {

    const {username: currentUsername, logout} = useAuth();
    const {notify} = useToast();
    const [busy, setBusy] = useState(false);

    const isSelf = user.username === currentUsername;
    const blockedDemote = isSelf && user.admin;

    const handleToggleAdmin = async () => {
        const granting = !user.admin;
        if (!window.confirm(granting
            ? `Make "${user.username}" an admin?`
            : `Revoke admin from "${user.username}"?`))
            return;

        setBusy(true);
        try {
            const res = await authClient.put("/User/SetAdmin", {id: user.id, admin: granting});
            onUpdated(new User(res.data.data));
            notify(`${granting ? "Promoted" : "Demoted"} "${user.username}"`);
        } catch (e: any) {
            const message = apiError(e, "Failed to change admin status");
            notify(message, "error");
        } finally {
            setBusy(false);
        }
    }

    const handleDelete = async () => {
        if (!window.confirm(isSelf
            ? `Delete your own account "${user.username}"? You will be logged out. This cannot be undone.`
            : `Delete "${user.username}"? This cannot be undone.`))
            return;

        setBusy(true);
        try {
            await authClient.delete(`/User/DeleteById/${user.id}`);
            notify(`Deleted "${user.username}"`, "info");
            if (isSelf)
                await logout();
            else
                onDeleted(user.id);
        } catch (e: any) {
            const message = apiError(e, "Failed to delete user");
            notify(message, "error");
        } finally {
            setBusy(false);
        }
    }

    return (
        <div className="userCard" data-testid="user-card">
            <div className="userInfo">
                <div className="info">
                    <p>Username: </p>
                    <p data-testid="user-username">{user.username}</p>
                </div>
                <div className="info">
                    <p>Email: </p>
                    <p>{user.email}</p>
                </div>
                <div className="info">
                    <p>Admin Status: </p>
                    <p data-testid="user-admin-status">{user.admin ? "True" : "False"}</p>
                </div>
            </div>
            <div className="buttons">
                <button className="button" data-testid="user-toggle-admin" type="button"
                        disabled={busy || blockedDemote}
                        title={blockedDemote ? "You cannot revoke your own admin status" : undefined}
                        onClick={handleToggleAdmin}>
                    {user.admin ? "Revoke Admin" : "Make Admin"}
                </button>
                <button className="button delete" data-testid="user-delete" type="button"
                        disabled={busy}
                        onClick={handleDelete}>
                    Delete
                </button>
            </div>
        </div>
    )
}
