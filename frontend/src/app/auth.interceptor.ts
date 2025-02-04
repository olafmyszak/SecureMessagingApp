import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const token = localStorage.getItem(environment.access_token);

    if (!token) {
        return next(req);
    }

    const clonedReq = req.clone({
        setHeaders: {
            Authorization: `Bearer ${token}`
        }
    });

    return next(clonedReq);
};
