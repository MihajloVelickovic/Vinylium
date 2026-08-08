import "../styles/UserCard.css"
import User from "../models/User.ts";
export const UserCard = ({user}: {user: User}) => {
    return (
        <div className="userCard">
            <div className="userInfo">
                <div className="info">
                    <p>Username: </p>
                    <p>{user.username}</p>
                </div>
                <div className="info">
                    <p>Email: </p>
                    <p>{user.email}</p>
                </div>
                <div className="info">
                    <p>Admin Status: </p>
                    <p>{user.admin ? "True" : "False"}</p>
                </div>
            </div>
            <div className="buttons">
                <button className="button">Toggle Admin</button>
                <button className="button">Reset Password</button>
                <button className="button delete">Delete</button>
            </div>
        </div>
    )
}