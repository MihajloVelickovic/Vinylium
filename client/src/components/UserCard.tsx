import "../styles/UserCard.css"
import User from "../models/User.ts";
export const UserCard = ({user}: {user: User}) => {
    return (
        <div className="userCard">
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
    )
}