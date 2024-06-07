import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { map } from 'rxjs';

export const authenticated: CanActivateFn = () => {
  const router = inject(Router);
  const userService = inject(UserService);

  return userService.userInfo$.pipe(
    map((info) => {
      if (!info.isAuthenticated) {
        return router.createUrlTree(['/login']);
      }

      return true;
    })
  );
};
