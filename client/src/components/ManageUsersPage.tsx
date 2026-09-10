import "../styles/ManageUsersPage.css"
import authClient from "../api/AuthClient.ts";
import {UserCard} from "./UserCard.tsx";
import {Filters} from "./Filters.tsx";
import {UserFilterFields} from "./UserFilterFields.tsx";
import {useUserFilters} from "../hooks/useUserFilters.ts";
import type User from "../models/User.ts";

export const ManageUsersPage = () => {

    const {users, setUsers, filters, setFilters, change, setChange, searchRef, loading, error} =
        useUserFilters(authClient);
    
    const handleUpdated = (updated: User) =>
        setUsers(prev => prev.map(u => u.id === updated.id ? updated : u));

    const handleDeleted = (id: string) =>
        setUsers(prev => prev.filter(u => u.id !== id));

    return (
        <>
            <Filters searchRef={searchRef} params={{filters, setFilters, change, setChange}}>
                <UserFilterFields params={{filters, setFilters, change, setChange}}/>
            </Filters>
            {
                loading ?
                    <h2 className="manage-users-message">Loading users...</h2> :
                error ?
                    <h2 className="manage-users-message manage-users-error">{error}</h2> :
                users.length === 0 ?
                    <h2 className="manage-users-message">No users found.</h2> :
                    <div className="users">
                        {
                            users.map(user => {
                                return <UserCard key={user.id} user={user}
                                                 onUpdated={handleUpdated}
                                                 onDeleted={handleDeleted}/>
                            })
                        }
                    </div>
            }
        </>
    )

}
