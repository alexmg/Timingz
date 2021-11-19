module.exports = {
    extends: ['@commitlint/config-conventional'],

    rules: {
        'type-enum':
            [2, 'always', [
                'revert', 'feat', 'fix', 'perf', 'docs', 'test', 'build', 'refactor', 'style', 'chore'
            ]],

        // Default rules we want to suppress:
        'body-leading-blank': [0, "always"],
        'body-max-line-length': [0, "always"],
        'footer-max-line-length': [0, "always"],
        'footer-leading-blank': [0, "always"],
        'subject-case': [0, "always", []],
        'subject-full-stop': [0, "never", '.']
    }
};