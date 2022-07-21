'use strict';

const gulp = require('gulp');
const cssnano = require('gulp-cssnano');
const sass = require('gulp-sass')(require('sass')); // uses dart-sass

var supported = [
    'last 2 versions',
    'safari >= 8',
    'ie >= 10',
    'ff >= 20',
    'ios 6',
    'android 4'
];

var compileCss = function () {
    return gulp.src(['Assets/scss/*.scss'])
        .pipe(sass())
        .pipe(cssnano({
            autoprefixer: { browsers: supported, add: true }
        }))
        .pipe(gulp.dest('./wwwroot/assets/css'));
};

gulp.task('sass-compile', compileCss);

gulp.task('sass:watch', function () {
    gulp.watch('./scss/*.scss', compileCss);
});