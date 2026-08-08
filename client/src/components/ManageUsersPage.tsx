import "../styles/ManageUsersPage.css"
import {useEffect, useState} from "react";
import User from "../models/User.ts";
import authClient from "../api/AuthClient.ts";
import {UserCard} from "./UserCard.tsx";
export const ManageUsersPage = () => {
    
    const [users, setUsers] = useState<Array<User>>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    
    useEffect(() => {
        const getUsers = async () => {
            const userList = new Array<User>();
            try {
                const res = await authClient.get("User/GetAllUsers");
                res.data.data.forEach((user: any) => {
                    userList.push(new User(user));
                });
            }
            catch(e){
                console.error(e);
            }
            return userList;
        }
        
        getUsers().then((u) => {
            setUsers(u);
            setLoading(false)
        }).catch((err) => {
            console.error(err);
            setError(err.response.data.message);
        });
        
    }, [])
    
    return (
        <div className="users">
            {
                (!loading && users.length > 0) ? 
                users.map(user => {
                    return <UserCard user={user}/>
                }) :
                    <h3>{error}</h3>
            }
        </div>
    )
    
}