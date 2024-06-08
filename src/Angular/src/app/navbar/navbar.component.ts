import { Component, inject } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import {
  BehaviorSubject,
  map,
  Observable,
  share,
  shareReplay,
  tap,
} from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { UserInfo, UserService } from '../services/user.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
})
export class NavbarComponent {
  isHandset$: Observable<boolean> = this.breakpointObserver
    .observe(Breakpoints.Handset)
    .pipe(
      map((result) => result.matches),
      shareReplay()
    );

  pageTitle$: Observable<string>;

  userInfo$ = inject(UserService).userInfo$.pipe(
    share({
      connector: () => {
        return new BehaviorSubject<UserInfo>({ isAuthenticated: false });
      },
      resetOnRefCountZero: true,
    })
  );

  constructor(
    private breakpointObserver: BreakpointObserver,
    private readonly activatedRoute: ActivatedRoute
  ) {
    this.pageTitle$ = this.activatedRoute.data.pipe(
      map((data) => data['pageTitle'])
    );
  }

  protected hasRole(userInfo: UserInfo) {
    return userInfo.isAuthenticated;
  }
}
