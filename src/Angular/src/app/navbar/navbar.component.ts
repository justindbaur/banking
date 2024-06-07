import { Component } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { map, Observable, shareReplay, tap } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

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

  constructor(
    private breakpointObserver: BreakpointObserver,
    private readonly activatedRoute: ActivatedRoute
  ) {
    this.pageTitle$ = this.activatedRoute.data.pipe(
      tap(v => console.log("data", v)),
      map((data) => data['pageTitle'])
    );
  }
}
