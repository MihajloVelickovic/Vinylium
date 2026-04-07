// PopOutCard.tsx
import React from 'react';
import {createPortal} from "react-dom";

interface PopOutCardProps {
    isOpen: boolean;
    onClose: () => void;
    title: string;
    backgroundImage: string;
    children: React.ReactNode;
}

const PopOutCard: React.FC<PopOutCardProps> = ({isOpen, onClose, title, backgroundImage, children}) => {
    if (!isOpen) return null; // Conditional rendering

    return createPortal(
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal-card" onClick={(e) => e.stopPropagation()}>
                <div className="modal-background" style={{
                    backgroundImage: `url(${backgroundImage})`
                }}/>
                <h2>{title}</h2>
                <button onClick={onClose} className="modal-close">×</button>
                <div className="modal-content">
                    {children}
                </div>
            </div>
        </div>,
        document.body
    );
};


export default PopOutCard;
