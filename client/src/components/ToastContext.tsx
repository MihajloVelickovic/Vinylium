import {createContext, useCallback, useContext, useRef, useState} from "react";
import type {ReactNode} from "react";
import "../styles/Toast.css";

type ToastKind = "success" | "error" | "info";

type Toast = {
    id: number;
    kind: ToastKind;
    message: string;
};

type ToastContextData = {
    notify: (message: string, kind?: ToastKind) => void;
    dismiss: (id: number) => void;
};

const ToastContext = createContext<ToastContextData>({} as ToastContextData);

const DURATION = 4000;

export const ToastProvider = ({children}: { children: ReactNode }) => {
    const [toasts, setToasts] = useState<Array<Toast>>([]);
    const nextId = useRef(0);
    const timers = useRef(new Map<number, number>());

    const dismiss = useCallback((id: number) => {
        setToasts(prev => prev.filter(t => t.id !== id));

        const timer = timers.current.get(id);
        if (timer !== undefined) {
            clearTimeout(timer);
            timers.current.delete(id);
        }
    }, []);

    const notify = useCallback((message: string, kind: ToastKind = "success") => {
        const id = nextId.current++;
        setToasts(prev => [...prev, {id, kind, message}]);
        timers.current.set(id, window.setTimeout(() => dismiss(id), DURATION));
    }, [dismiss]);

    return (
        <ToastContext value={{notify, dismiss}}>
            {children}
            <div className="toast-stack" role="status" aria-live="polite">
                {toasts.map(t => (
                    <button className={`toast toast-${t.kind}`}
                            key={t.id}
                            type="button"
                            title="Dismiss"
                            onClick={() => dismiss(t.id)}>
                        {t.message}
                    </button>
                ))}
            </div>
        </ToastContext>
    );
};

export const useToast = () => useContext(ToastContext);
